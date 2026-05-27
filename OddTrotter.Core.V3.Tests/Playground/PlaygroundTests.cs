using Fx;

namespace Playground
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.Graph.CalendarEventsContext;
    using OddTrotter.Odata.v4_01.StrongConventionContext;

    using static Playground.PlaygroundTests;

    [TestClass]
    public sealed class PlaygroundTests
    {
        [TestMethod]
        public async Task ReadingFromDeadNetworkStream()
        {
            //// TODO to repro this, you need to turn off wifi, set a breakpoint for after `readasstreamasync`, run the test, let the breakpoint get hit, disconnect the network cable, and continue from the breakpoint

            using (var handler = new SocketsHttpHandler())
            {
                //// TODO by default (i.e. when no handler is provided), `httpclient` doesn't appear to have a timeout for when the underlying connection is no longer available (e.g. due to disconnected hardware), so we need to set a timeout; 5 seconds is probably too short for practical uses
                handler.PooledConnectionIdleTimeout = TimeSpan.FromSeconds(5);
                using (var httpClient = new HttpClient(handler, false))
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36 Edg/144.0.3719.104");

                    var url = "https://cdimage.debian.org/debian-cd/current/amd64/iso-cd/debian-13.3.0-amd64-netinst.iso";
                    ////var url = "https://chuangtzu.ftp.acc.umu.se/debian-cd/current/amd64/iso-cd/debian-13.3.0-amd64-netinst.iso";
                    ////var url = "https://www.google.com";


                    //// TODO `httpcompletionoption` needs to be set so that only the headers are read; otherwise, we will sit at this line until the entire payload is read into memory
                    using (var httpResponse = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        using (var contentStream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        {
                            var buffer = new byte[1024];
                            int read = -1;

                            while (read != 0)
                            {
                                try
                                {
                                    read = await contentStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                                }
                                catch (IOException ioException)
                                {
                                    //// TODO `readasync` throws `ioexception` (which i think is good that they actually chose to preserve the `stream` contract), but when the underlying connection has an issue, it has an inner `socketexception`, so we, knowing that the stream is actually coming from an httpclient, can use this inner exception to give our caller a better experience
                                    if (ioException.InnerException is SocketException socketException)
                                    {
                                        throw new HttpRequestException("TODO", socketException);
                                    }

                                    throw;
                                }
                            }
                        }
                    }
                }
            }
        }

        /*public static void Foo(Calendar<INetwork> calendarProvider)
        {
            calendarProvider.Events.Case
        }

        public class Calendar<T>
            where T : ICase
        {
            public StringValue<T> Id { get; }

            public IOdataCollection<T, CalendarEvent<T>> Events { get; }
        }

        public class CalendarEvent<T>
            where T : ICase
        {
            public StringValue<T> Id { get; }

            public BoolValue<T> IsCanceled { get; }

            public DateTimeValue<T> Start { get; }

            public IOdataCollection<T, CalendarEvent<T>> Instances { get; }
        }

        public class StringValue<T>
            where T : ICase
        {
            T Case { get; }
        }

        public class DateTimeValue<T>
            where T : ICase
        {
            T Case { get; }
        }

        public class BoolValue<T>
            where T : ICase
        {
            T Case { get; }
        }

        public interface IOdataCollection<TCase, out TElement>
            where TCase : ICase
        {
            TCase Case { get; }
        }

        public interface ISingleValue<T>
        {
            T Value { get; }
        }

        public interface ICase
        {
        }

        public interface INetwork : ICase
        {
        }

        public interface IMemory : ICase
        {
        }*/

        [TestMethod]
        public void ParsePredicate()
        {
            Assert.IsTrue(TryTranslateToSeriesMaster(calendarEvent => calendarEvent.Subject == "todo list", out var translated));

            var graphCalendarEvent = new CalendarEvent(
                "id",
                "subject",
                new BodyStructure(
                    "content"),
                new TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"),
                false,
                "series",
                new TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"));

            Assert.IsFalse(translated(graphCalendarEvent));

            graphCalendarEvent = new CalendarEvent(
                "id",
                "todo list",
                new BodyStructure(
                    "content"),
                new TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"),
                false,
                "series",
                new TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"));

            Assert.IsTrue(translated(graphCalendarEvent));
        }

        private static bool TryTranslateToSeriesMaster(Expression<Func<OddTrotter.CalendarEventsContext.CalendarEvent, bool>> predicate, out Func<CalendarEvent, bool> seriesMasterPredicate)
        {
            // note: this doesn't ever return a null predicate; we still translate when it's not a series master

            var calendarEventParameter = predicate.Parameters[0];

            var translatedcalendarEventParameter = Expression.Parameter(typeof(CalendarEvent), calendarEventParameter.Name);
            var visitor = new ExpressionVisitor(calendarEventParameter, translatedcalendarEventParameter);
            var translatedBody = visitor.Visit(predicate.Body);

            Expression<Func<CalendarEvent, bool>> translated = calendarEvent => true;
            translated = translated.Update(translatedBody, new[] { translated.Parameters[0] }); //// new[] { translatedcalendarEventParameter });

            var lambda = Expression.Lambda<Func<CalendarEvent, bool>>(translatedBody, translatedcalendarEventParameter);
            translated = lambda;

            

            seriesMasterPredicate = translated.Compile();

            return visitor.ApplicableToSeriesMaster; //// TODO maybe this should return the non-compiled version? //// TODO if you don't compile, you don't get error handling on the new predicate
        }

        private sealed class ExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
        {
            private readonly ParameterExpression originalCalendarEventParamter;
            private readonly ParameterExpression translatedCalendarEventExpression;

            public ExpressionVisitor(ParameterExpression originalCalendarEventParamter, ParameterExpression translatedCalendarEventExpression)
            {
                this.originalCalendarEventParamter = originalCalendarEventParamter;
                this.translatedCalendarEventExpression = translatedCalendarEventExpression;
                ApplicableToSeriesMaster = false;
            }

            public bool ApplicableToSeriesMaster { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return translatedCalendarEventExpression;

                /*if (object.ReferenceEquals(node, this.originalCalendarEventParamter))
                {
                    return this.translatedCalendarEventExpression;
                }

                return base.VisitParameter(node);*/
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (ReferenceEquals(node.Expression, originalCalendarEventParamter))
                {
                    if (SeriesMasterAdapters.TryGetValue(node.Member.Name, out var seriesMasterExpression))
                    {
                        seriesMasterExpression = Visit(seriesMasterExpression);

                        ApplicableToSeriesMaster = true;
                        return seriesMasterExpression;
                    }

                    if (CalendarEventAdapters.TryGetValue(node.Member.Name, out var calendarEventExpression))
                    {
                        calendarEventExpression = Visit(calendarEventExpression);

                        return calendarEventExpression;
                    }
                }

                return base.VisitMember(node);
            }

            private static Expression<Func<CalendarEvent, string>> SubjectExpression { get; } = calendarEvent => calendarEvent.Subject;
            private static Expression<Func<CalendarEvent, string>> IdExpression { get; } = calendarEvent => calendarEvent.Id;
            private static Expression<Func<CalendarEvent, string>> BodyExpression { get; } = calendarEvent => calendarEvent.Body.Content;
            private static Expression<Func<CalendarEvent, bool>> IsCancelledExpression { get; } = calendarEvent => calendarEvent.IsCancelled;
            private static Expression<Func<CalendarEvent, string>> TypeExpression { get; } = calendarEvent => calendarEvent.Type;

            private static IReadOnlyDictionary<string, Expression> SeriesMasterAdapters { get; } = new Dictionary<string, Expression>()
            {
                { "Subject", SubjectExpression.Body },
                { "Id", IdExpression.Body },
                { "Body", BodyExpression.Body },
                { "IsCancelled", IsCancelledExpression.Body },
                { "Type", TypeExpression.Body },
            };

            private static Expression<Func<CalendarEvent, DateTime>> StartExpression { get; } = calendarEvent => calendarEvent.Start.DateTime;
            private static Expression<Func<CalendarEvent, DateTime>> EndExpression { get; } = calendarEvent => calendarEvent.End.DateTime;

            private static IReadOnlyDictionary<string, Expression> CalendarEventAdapters { get; } = new Dictionary<string, Expression>()
            {
                { "Start", StartExpression.Body },
                { "End", EndExpression.Body },
            };
        }






        [TestMethod]
        public void EitherFactoryTest()
        {
            //// TODO i don't really know where you're going with this; you were thinking that you could use this to "swap" the use of one `ieither` implementation with another across the whole project; not sure that really works though
        }


        private sealed class EitherFactory : IEitherFactory
        {
            public IEitherWithFactory<TLeft, TRight> Create<TLeft, TRight>(TLeft value)
            {
                throw new NotImplementedException();
            }

            public IEitherWithFactory<TLeft, TRight> Create<TLeft, TRight>(TRight value)
            {
                throw new NotImplementedException();
            }
        }



        public interface IEitherWithFactory<out TLeft, out TRight> : IEither<TLeft, TRight>
        {
            static abstract IEitherFactory Factory { get; }
        }

        public interface IEitherFactory
        {
            IEitherWithFactory<TLeft, TRight> Create<TLeft, TRight>(TLeft value);

            IEitherWithFactory<TLeft, TRight> Create<TLeft, TRight>(TRight value);
        }














        public interface IDeconstructable<TLeft, TRight>
        {
            bool Deconstruct([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right);
        }

        

        static void DoDeconstruct(IDeconstructable<string, int> deconstructable)
        {
            var (isLeft, left, right) = deconstructable;
        }
    }

    public static class DeconstructExtensions
    {
        public static void Deconstruct<TLeft, TRight>(this PlaygroundTests.IDeconstructable<TLeft, TRight> deconstructable, out bool isLeft, out TLeft? left, out TRight? right)
        {
            isLeft = deconstructable.Deconstruct(out left, out right);
        }
    }

    public static class FactoryExtensions
    {
        public readonly ref struct EmptyLeft<TLeft>
        {
            private readonly IEitherFactory factory;

            public EmptyLeft(IEitherFactory factory)
            {
                this.factory = factory;
            }

            public IEither<TLeft, TRight> Right<TRight>(TRight value)
            {
                return factory.Create<TLeft, TRight>(value);
            }
        }

        public static EmptyLeft<TLeft> Left<TLeft>(this IEitherFactory factory)
        {
            return new EmptyLeft<TLeft>(factory);
        }

        public readonly ref struct EmptyRight<TRight>
        {
            private readonly IEitherFactory factory;

            public EmptyRight(IEitherFactory factory)
            {
                this.factory = factory;
            }

            public IEither<TLeft, TRight> Left<TLeft>(TLeft value)
            {
                return factory.Create<TLeft, TRight>(value);
            }
        }

        public static EmptyRight<TRight> Right<TRight>(this IEitherFactory factory)
        {
            return new EmptyRight<TRight>(factory);
        }
    }

    [TestClass]
    public sealed class OdataContexts
    {
        internal static async Task Foo(IUsersSource usersSource)
        {
            var managerSource = usersSource.Get("00000000-0000-0000-0000-000000000000");
            var managerContext = managerSource.Get();
            managerContext = managerContext.Expand(manager => manager.DirectReports());

            var manager = await managerContext.Evaluate();

            if (manager.DisplayName.IsProvided(out var displayName))
            {
                Console.WriteLine(displayName);
            }






            usersSource
                .Get()
                .Filter(user => user.DisplayName.Contains("foo"))
                .Filter(user => user.DirectReports.Any(directReport => directReport.DisplayName.Contains(user.DisplayName)));
        }












        //// TODO this is a bit much; there are (at least) 3 contexts where an EDM structured type may be used: payload, URL path, and expression; and you need a different representation of the structured type in each of those contexts


        internal interface IUserExpression
        {
            IStringExpression Id { get; }

            IStringExpression DisplayName { get; }

            ICollectionExpression<IUserExpression> DirectReports { get; }
        }

        internal interface ICollectionExpression<T>
        {
            bool Any(Expression<Func<T, bool>> expression);
        }

        internal interface IStringExpression
        {
            bool Contains(string value);

            bool Contains(IStringExpression stringExpression);
        }














        internal interface IUsersSource
        {
            IUsersContext Get();

            IUserSource Get(string id);
        }

        internal interface IUsersContext
        {
            ITask<IQueryResult<IEither<User, Exception>, Exception>> Evaluate();

            IUsersContext Filter(Expression<Func<IUserExpression, bool>> filter);
        }

        internal interface IUserSource
        {
            IUserContext Get();

            IUsersSource DirectReports();

            IStringSource Id();

            IStringSource DisplayName();
        }

        internal interface IStringSource
        {
            IStringContext Get();
        }

        internal interface IStringContext
        {
            Task<string> Evaluate();
        }

        internal interface IUserContext
        {
            Task<User> Evaluate();

            IUserContext Expand<T>(Expression<Func<IUserSource, T>> expander); //// TODO can you make this target only navigation properties?
        }


































        internal sealed class User
        {
            internal User(
                string id, 
                string displayName, 
                IEnumerable<User> directReports)
            {
                Id = Property.Provided(id);
                DisplayName = Property.Provided(displayName);
                DirectReports = Property.Navigation(directReports);
            }

            internal User()
            {
                Id = Property.NotProvided<string>();
                DisplayName = Property.NotProvided<string>();
                DirectReports = Property.NotProvided<IEnumerable<User>>();
            }

            public Provided<string> Id { get; set; }
            public Provided<string> DisplayName { get; set; }
            public NavigationProperty<Provided<IEnumerable<User>>> DirectReports { get; set; } //// TODO there should be an `odatacollection` or something that has the `nextlink`
        }

        internal static class Property
        {
            internal static Provided<T>.No NotProvided<T>()
            {
                return OdataContexts.Provided<T>.No.Instance;
            }

            internal static Provided<T>.Yes Provided<T>(T value)
            {
                return new Provided<T>.Yes(value);
            }

            internal static NavigationProperty<Provided<T>> Navigation<T>(T value)
            {
                return new NavigationProperty<Provided<T>>(Provided(value));
            }
        }

        internal sealed class NavigationProperty<T>
        {
            public NavigationProperty(T value)
            {
                Value = value;
            }

            public T Value { get; }

            //// TODO add things like odata.context here
        }

        internal abstract class Provided<T>
        {
            private Provided()
            {
            }

            public bool IsProvided([MaybeNullWhen(false)] out T value)
            {
                if (this is Yes yes)
                {
                    value = yes.Value;
                    return true;
                }

                value = default;
                return false;
            }

            internal sealed class Yes : Provided<T>
            {
                internal Yes(T value)
                {
                    Value = value;
                }

                public T Value { get; }
            }

            internal sealed class No : Provided<T>
            {
                private No()
                {
                }

                public static No Instance { get; } = new No();

                public static implicit operator NavigationProperty<Provided<T>>(No no)
                {
                    return new NavigationProperty<Provided<T>>(no);
                }
            }

            public static implicit operator T(Provided<T> provided)
            {
                return default!;
            }
        }









    }

    [TestClass]
    public sealed class StaticMemberRepro
    {
        [TestMethod]
        public void Test()
        {
            var foo = FooExtensions.GetFoo();
            foo.DoWork2();
            foo.DoWork2();
        }
    }

    public static class FooExtensions
    {
        public static Foo GetFoo()
        {
            return null!;
        }

        public static void DoWork2<TFoo>(this TFoo foo)
            where TFoo : IFoo
        {
            Foo.DoWork();
        }
    }

    public interface IFoo
    {
        static abstract void DoWork();
    }

    public sealed class Foo : IFoo
    {
        public static int Value = -1;

        public static void DoWork()
        {
            Console.WriteLine(Value);
            ++Value;
        }
    }
}
