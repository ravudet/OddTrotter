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
    using System.Text;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.QueryContext;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.Graph.CalendarEventsContext;
    using OddTrotter.Odata.v4_01.StrongConventionContext;

    using static Playground.PlaygroundTests;
    using static Playground.TopLayer.Odata.MetadataDto;

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
            var foo = FooExtensions.GetFoo<Foo>();
            foo.DoWork2();

            var foo2 = FooExtensions.GetFoo<Foo>();
            foo2.DoWork2();
        }

        private const string data =
"""
{
    "true": true,
    "false": false,
    "number": 1234,
    "string": "asdf",
    "null": null,
    "object": {
        "true": true,
        "false": false,
        "number": 1234,
        "string": "asdf",
        "null": null
    },
    "emptyObject": {},
    "emptyArray": [],
    "array": [
        {
            "true": true,
            "false": false,
            "number": 1234,
            "string": "asdf",
            "null": null
        }
    ]
}
""";

        [TestMethod]
        public async Task Test2()
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                stream.Position = 0;
                var context = await ReaderContext.FromStream(stream, new byte[stream.Length]).ConfigureAwait(false);
                var reader = Readers.Create();

                var whitespaceReader = reader.MoveTry1(context);
                var valueReader = whitespaceReader.MoveTry2(context);
                var valueToken = valueReader.MoveTry3(context);
                Assert.IsTrue(valueToken.TryObject(out var @object));
                var objectStart = @object.MoveTry1(context);
                var whitespacereader2 = objectStart.MoveTry4(context);
                var membersReader = whitespacereader2.MoveTry2(context);
                var membersToken = membersReader.MoveTry3(context);
                Assert.IsTrue(membersToken.TrySome(out var firstMemberReader));

                // true
                var memberReader = firstMemberReader.MoveTry1(context);
                var stringReader = memberReader.MoveTry1(context);
                var charsReader = stringReader.MoveTry1(context);
                var whitespaceReader3 = charsReader.MoveTry2(context);
                var whitespaceReader4 = whitespaceReader3.MoveTry2(context);
                var valueReader2 = whitespaceReader4.MoveTry2(context);
                var valueToken2 = valueReader2.MoveTry3(context);
                Assert.IsFalse(valueToken2.TryTrue(out var @true)); //// TODO should be true
            }
        }
    }

    public static class FooExtensions
    {
        public static TFoo GetFoo<TFoo>()
        {
            return default!;
        }

        public static void DoWork2<TFoo>(this TFoo foo)
            where TFoo : IFoo
        {
            TFoo.DoWork();
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

    public sealed class ReaderContext
    {
        private ReaderContext(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes)
        {
            Stream = stream;
            Buffer = buffer;
            CurrentByteIndex = currentByteIndex;
            ValidBytes = validBytes;
        }

        public Stream Stream { get; }
        public byte[] Buffer { get; }
        public int CurrentByteIndex { get; set; }
        public int ValidBytes { get; set; }

        public static async ValueTask<ReaderContext> FromStream(Stream stream, byte[] buffer)
        {
            var readerContext = new ReaderContext(stream, buffer, 0, 0);
            await readerContext.Read().ConfigureAwait(false);
            return readerContext;
        }
    }

    public static class ReaderContextExtensions
    {
        public static async ValueTask Read(this ReaderContext readerContext)
        {
            readerContext.ValidBytes = await readerContext.Stream.ReadAsync(readerContext.Buffer.AsMemory()).ConfigureAwait(false);
            readerContext.CurrentByteIndex = 0;
        }
    }



    public interface IMoveReader<TCurrentReader, TNextReader>
        where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TNextReader nextReader);
    }

    public interface IValueReader<TCurrentReader, TNextReader, TValue>
        where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out TValue value);
    }

    public interface IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out TValue value, out TContext context);

        static abstract bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out TValue value, ref TContext context);
    }

    public interface ITokenReader<TCurrentReader, TToken>
        where TCurrentReader : ITokenReader<TCurrentReader, TToken>
    {
        static abstract bool TryMove(ReaderContext readerContext, out TToken token);
    }


    public sealed class JsonReader : IMoveReader<JsonReader, WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out WhitespaceReader<ValueReader<WhitespaceReader<Nothing>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class WhitespaceReader<TNextReader> : IContinuableValueReader<WhitespaceReader<TNextReader>, TNextReader, List<WhitespaceToken>, List<WhitespaceToken>>
    {
        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out List<WhitespaceToken> value, ref List<WhitespaceToken> context)
        {
            /*while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    nextReader = default!; //// TODO !
                    value = default!; //// TODO !
                    return false;
                }

                if (!WhitespaceToken.TryCreate(readerContext.Buffer[readerContext.CurrentByteIndex], out var whitespace))
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                context.Add(whitespace);
            }

            nextReader = default!; //// TODO !
            value = context;
            return true;*/

            nextReader = default!;
            value = default!;
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out List<WhitespaceToken> value, out List<WhitespaceToken> context)
        {
            /*context = new List<WhitespaceToken>();
            return WhitespaceReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);*/

            nextReader = default!;
            value = default!;
            context = default!;
            return true;
        }
    }

    public readonly struct WhitespaceToken
    {
        public static bool TryCreate(byte @char, out WhitespaceToken whitespaceToken)
        {
            switch (@char)
            {
                case 0x20:
                case 0x09:
                case 0x0A:
                case 0x0D:
                    whitespaceToken = new WhitespaceToken(@char);
                    return true;
                default:
                    whitespaceToken = default;
                    return false;
            }
        }

        private WhitespaceToken(byte @char)
        {
            this.Char = @char;
        }

        public byte Char { get; }
    }

    public sealed class ValueReader<TNextReader> : ITokenReader<ValueReader<TNextReader>, ValueToken<TNextReader>>
    {
        private static int ValueCount = -1;

        public static bool TryMove(ReaderContext readerContext, out ValueToken<TNextReader> token)
        {
            switch (ValueCount)
            {
                case -1:
                    token = ValueToken<TNextReader>.Object();
                    break;
                case 0:
                    token = ValueToken<TNextReader>.True();
                    break;
                case 1:
                    token = ValueToken<TNextReader>.False();
                    break;
                case 2:
                    token = ValueToken<TNextReader>.Number();
                    break;
                case 3:
                    token = ValueToken<TNextReader>.String();
                    break;
                case 4:
                    token = ValueToken<TNextReader>.Null();
                    break;
                case 5:
                    token = ValueToken<TNextReader>.Object();
                    break;
                case 6:
                    token = ValueToken<TNextReader>.True();
                    break;
                case 7:
                    token = ValueToken<TNextReader>.False();
                    break;
                case 8:
                    token = ValueToken<TNextReader>.Number();
                    break;
                case 9:
                    token = ValueToken<TNextReader>.String();
                    break;
                case 10:
                    token = ValueToken<TNextReader>.Null();
                    break;
                case 11:
                    token = ValueToken<TNextReader>.Object();
                    break;
                case 12:
                    token = ValueToken<TNextReader>.Array();
                    break;
                case 13:
                    token = ValueToken<TNextReader>.Array();
                    break;
                case 14:
                    token = ValueToken<TNextReader>.Object();
                    break;
                case 15:
                    token = ValueToken<TNextReader>.True();
                    break;
                case 16:
                    token = ValueToken<TNextReader>.False();
                    break;
                case 17:
                    token = ValueToken<TNextReader>.Number();
                    break;
                case 20:
                    token = ValueToken<TNextReader>.String();
                    break;
                case 19:
                    token = ValueToken<TNextReader>.Null();
                    break;
                default:
                    throw new Exception("TODO invalid");
            }

            ++ValueCount;
            return true;


        }
    }

    public readonly struct ValueToken<TNextReader>
    {
        private int type { get; init; }

        public static ValueToken<TNextReader> False()
        {
            return new ValueToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static ValueToken<TNextReader> Null()
        {
            return new ValueToken<TNextReader>()
            {
                type = 2,
            };
        }

        public static ValueToken<TNextReader> True()
        {
            return new ValueToken<TNextReader>()
            {
                type = 3,
            };
        }

        public static ValueToken<TNextReader> Object()
        {
            return new ValueToken<TNextReader>()
            {
                type = 4,
            };
        }

        public static ValueToken<TNextReader> Array()
        {
            return new ValueToken<TNextReader>()
            {
                type = 5,
            };
        }

        public static ValueToken<TNextReader> Number()
        {
            return new ValueToken<TNextReader>()
            {
                type = 6,
            };
        }

        public static ValueToken<TNextReader> String()
        {
            return new ValueToken<TNextReader>()
            {
                type = 7,
            };
        }

        public bool TryTrue(out TrueReader<TNextReader> trueReader)
        {
            trueReader = default!; //// TODO !
            return this.type == 3;
        }

        public bool TryObject(out ObjectReader<TNextReader> objectReader)
        {
            objectReader = default!; //// TODO !
            return this.type == 4;
        }
    }

    public sealed class ObjectReader<TNextReader> : IMoveReader<ObjectReader<TNextReader>, ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out ObjectStartReader<WhitespaceReader<MembersReader<WhitespaceReader<ObjectEndReader<TNextReader>>>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class ObjectStartReader<TNextReader> : IValueReader<ObjectStartReader<TNextReader>, TNextReader, ObjectStartToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out ObjectStartToken value)
        {
            /*nextReader = default!;
            return Json6.Helpers.TryReadChar(readerContext, '{');*/

            nextReader = default!;
            return true;
        }
    }

    public readonly struct ObjectStartToken
    {
    }

    public sealed class MembersReader<TNextReader> : ITokenReader<MembersReader<TNextReader>, MembersToken<TNextReader>>
    {
        private static int MembersCount = 0;

        public static bool TryMove(ReaderContext readerContext, out MembersToken<TNextReader> token)
        {
            switch (MembersCount)
            {
                case 0:
                    token = MembersToken<TNextReader>.Some();
                    break;
                case 1:
                    token = MembersToken<TNextReader>.None();
                    break;
                case 2:
                    token = MembersToken<TNextReader>.Some();
                    break;
                default:
                    throw new Exception("TODO invalid");
            }

            ++MembersCount;
            return true;

            /*if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
            {
                token = default;
                return false;
            }

            if (readerContext.ValidBytes == 0)
            {
                throw new Exception("TODO invalid JSON");
            }

            if (readerContext.Buffer[readerContext.CurrentByteIndex] == '"')
            {
                token = MembersToken<TNextReader>.Some();
            }
            else
            {
                token = MembersToken<TNextReader>.None();
            }

            return true;*/
        }
    }

    public readonly struct MembersToken<TNextReader>
    {
        private int type { get; init; }

        public static MembersToken<TNextReader> None()
        {
            return new MembersToken<TNextReader>()
            {
                type = 1,
            };
        }

        public static MembersToken<TNextReader> Some()
        {
            return new MembersToken<TNextReader>()
            {
                type = 2,
            };
        }

        public bool TryNone([MaybeNullWhen(false)] out TNextReader nextReader)
        {
            nextReader = default;
            return this.type == 1;
        }

        public bool TrySome(out FirstMemberReader<TNextReader> firstMemberReader)
        {
            firstMemberReader = default!; //// TODO !
            return this.type == 2;
        }
    }

    public sealed class FirstMemberReader<TNextReader> : IMoveReader<FirstMemberReader<TNextReader>, MemberReader<SubsequentMembersReader<TNextReader>>>
    {
        public static bool TryMove(ReaderContext readerContext, out MemberReader<SubsequentMembersReader<TNextReader>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class MemberReader<TNextReader> : IMoveReader<MemberReader<TNextReader>, StringReader<WhitespaceReader<WhitespaceReader<ValueReader<TNextReader>>>>>
    {
        public static bool TryMove(ReaderContext readerContext, out StringReader<WhitespaceReader<WhitespaceReader<ValueReader<TNextReader>>>> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class StringReader<TNextReader> : IMoveReader<StringReader<TNextReader>, CharsReader<TNextReader>>
    {
        public static bool TryMove(ReaderContext readerContext, out CharsReader<TNextReader> nextReader)
        {
            nextReader = default!; //// TODO !
            return true;
        }
    }

    public sealed class StringDelimiterReader<TNextReader> : IValueReader<StringDelimiterReader<TNextReader>, TNextReader, StringDelimiterToken>
    {
        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out StringDelimiterToken value)
        {
            /*nextReader = default!; //// TODO !
            return Json6.Helpers.TryReadChar(readerContext, '"');*/

            nextReader = default!;
            value = default!;
            return true;
        }
    }

    public readonly struct StringDelimiterToken
    {
    }

    public sealed class CharsReader<TNextReader> : IContinuableValueReader<CharsReader<TNextReader>, TNextReader, List<CharToken>, (List<CharToken>, bool)>
    {
        public static bool TryContinue(ReaderContext readerContext, out TNextReader nextReader, out List<CharToken> value, ref (List<CharToken>, bool) context)
        {
            /*while (true)
            {
                if (readerContext.ValidBytes == 0)
                {
                    // no more bytes to read
                    break;
                }

                if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                {
                    // read more from the stream
                    nextReader = default!; //// TODO !
                    context = (context.Item1, false);
                    value = context.Item1;
                    return false;
                }

                var currentByte = readerContext.Buffer[readerContext.CurrentByteIndex];
                if (currentByte == 0x5C)
                {
                    ++readerContext.CurrentByteIndex;
                    if (readerContext.CurrentByteIndex >= readerContext.ValidBytes)
                    {
                        nextReader = default!; //// TODO !
                        context = (context.Item1, true); //// TODO you need to leverage the context that we are in the middle of an escape
                        value = context.Item1;
                        return false;
                    }

                    if (readerContext.ValidBytes == 0)
                    {
                        throw new Exception("TODO invalid JSON");
                    }

                    throw new Exception("TODO escaped characters are not yet supported");
                }

                if (!CharToken.TryUnescaped(currentByte, out var @char))
                {
                    break;
                }

                ++readerContext.CurrentByteIndex;
                context.Item1.Add(@char);
            }

            nextReader = default!; //// TODO !
            value = context.Item1;
            return true;*/

            nextReader = default!;
            value = default!;
            context = default!;
            return true;
        }

        public static bool TryMove(ReaderContext readerContext, out TNextReader nextReader, out List<CharToken> value, out (List<CharToken>, bool) context)
        {
            /*context = (new List<CharToken>(), false);
            return CharsReader<TNextReader>.TryContinue(readerContext, out nextReader, out value, ref context);*/

            nextReader = default!;
            value = default!;
            context = default!;
            return true;
        }
    }

    public readonly struct CharToken
    {
        private int type { get; init; }

        public byte Char { get; private init; }

        public static bool TryUnescaped(byte @char, out CharToken charToken)
        {
            if (!IsValid(@char))
            {
                charToken = default;
                return false;
            }

            charToken = new CharToken()
            {
                type = 1,
                Char = @char,
            };
            return true;
        }

        private static bool IsValid(byte @char)
        {
            return
                (@char >= 0x20 && @char <= 0x21) ||
                (@char >= 0x23 && @char <= 0x5B) ||
                (@char >= 0x5D); //// TODO the upper bound here in the standard is not actually a valid byte...
        }
    }

    public sealed class SubsequentMembersReader<TNextReader>
    {
    }

    public sealed class ObjectEndReader<TNextReader>
    {
    }

    public sealed class FalseReader<TNextReader>
    {
    }

    public sealed class NullReader<TNextReader>
    {
    }

    public sealed class TrueReader<TNextReader>
    {
    }

    public sealed class ArrayReader<TNextReader>
    {
    }

    public sealed class NumberReader<TNextReader>
    {
    }
    public static class Readers
    {
        public static JsonReader Create()
        {
            return null!; //// TODO !
        }

        public static void DoWork<T1, T2>(
            this T1 t1,
            out T2 t2)
        {
            t2 = default!;
        }

        public static TNextReader MoveTry1<TCurrentReader, TNextReader>(
            this IMoveReader<TCurrentReader, TNextReader> moveReader,
            ReaderContext readerContext)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            TCurrentReader.TryMove(readerContext, out var nextReader);
            return nextReader;
        }

        public static bool TryMove1<TCurrentReader, TNextReader>(
            this IMoveReader<TCurrentReader, TNextReader> moveReader,
            ReaderContext readerContext,
            out TNextReader nextReader)
            where TCurrentReader : IMoveReader<TCurrentReader, TNextReader>
        {
            return TCurrentReader.TryMove(readerContext, out nextReader);
        }

        public static TNextReader MoveTry2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> continuableValueReader,
            ReaderContext readerContext)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            TCurrentReader.TryMove(readerContext, out var nextReader, out _, out _);
            return nextReader;
        }

        public static bool TryMove2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> continuableValueReader,
            ReaderContext readerContext,
            out TNextReader nextReader,
            out TValue value,
            out TContext context)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            return TCurrentReader.TryMove(readerContext, out nextReader, out value, out context);
        }

        public static bool TryContinue2<TCurrentReader, TNextReader, TValue, TContext>(
            this IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext> continuableValueReader,
            ReaderContext readerContext,
            out TNextReader nextReader,
            out TValue value,
            ref TContext context)
            where TCurrentReader : IContinuableValueReader<TCurrentReader, TNextReader, TValue, TContext>
        {
            return TCurrentReader.TryContinue(readerContext, out nextReader, out value, ref context);
        }

        public static TToken MoveTry3<TCurrentReader, TToken>(this ITokenReader<TCurrentReader, TToken> tokenReader, ReaderContext readerContext)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>
        {
            TCurrentReader.TryMove(readerContext, out var token);
            return token;
        }

        public static bool TryMove3<TCurrentReader, TToken>(this ITokenReader<TCurrentReader, TToken> tokenReader, ReaderContext readerContext, out TToken token)
            where TCurrentReader : ITokenReader<TCurrentReader, TToken>
        {
            return TCurrentReader.TryMove(readerContext, out token);
        }

        public static TNextReader MoveTry4<TCurrentReader, TNextReader, TValue>(this IValueReader<TCurrentReader, TNextReader, TValue> valueReader, ReaderContext readerContext)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
        {
            TCurrentReader.TryMove(readerContext, out var nextReader, out _);
            return nextReader;
        }

        public static bool TryMove4<TCurrentReader, TNextReader, TValue>(this IValueReader<TCurrentReader, TNextReader, TValue> valueReader, ReaderContext readerContext, out TNextReader nextReader, out TValue value)
            where TCurrentReader : IValueReader<TCurrentReader, TNextReader, TValue>
        {
            return TCurrentReader.TryMove(readerContext, out nextReader, out value);
        }


        /*public static async Task<(ReaderContext, TNextReader)> Move2<TNextReader>(this WhitespaceReader<TNextReader> whitespaceReader, ReaderContext readerContext)
        {
            while (true)
            {
                var whitespaceToken = await whitespaceReader.Move31(readerContext).ConfigureAwait(false);
                if (whitespaceToken.TryMore(out var whitespaceCharReader))
                {
                    whitespaceReader = await whitespaceCharReader.Move4(readerContext).ConfigureAwait(false);
                }
                else if (whitespaceToken.TryNone(out var nextReader))
                {
                    return (readerContext, nextReader);
                }
            }
        }*/

    }
}
