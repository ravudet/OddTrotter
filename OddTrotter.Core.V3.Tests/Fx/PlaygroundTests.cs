namespace Fx
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Fx.Either;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.Odata.v4_01.StrongConventionContext;

    using static Fx.PlaygroundTests;

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

            var graphCalendarEvent = new OddTrotter.Graph.CalendarEventsContext.CalendarEvent(
                "id",
                "subject",
                new OddTrotter.Graph.CalendarEventsContext.BodyStructure(
                    "content"),
                new OddTrotter.Graph.CalendarEventsContext.TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"),
                false,
                "series",
                new OddTrotter.Graph.CalendarEventsContext.TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"));

            Assert.IsFalse(translated(graphCalendarEvent));

            graphCalendarEvent = new OddTrotter.Graph.CalendarEventsContext.CalendarEvent(
                "id",
                "todo list",
                new OddTrotter.Graph.CalendarEventsContext.BodyStructure(
                    "content"),
                new OddTrotter.Graph.CalendarEventsContext.TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"),
                false,
                "series",
                new OddTrotter.Graph.CalendarEventsContext.TimeStructure(
                    DateTime.Parse("2026-02-19"),
                    "UTC"));

            Assert.IsTrue(translated(graphCalendarEvent));
        }

        private static bool TryTranslateToSeriesMaster(Expression<Func<OddTrotter.CalendarEventsContext.CalendarEvent, bool>> predicate, out Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, bool> seriesMasterPredicate)
        {
            // note: this doesn't ever return a null predicate; we still translate when it's not a series master

            var calendarEventParameter = predicate.Parameters[0];

            var translatedcalendarEventParameter = Expression.Parameter(typeof(OddTrotter.Graph.CalendarEventsContext.CalendarEvent), calendarEventParameter.Name);
            var visitor = new ExpressionVisitor(calendarEventParameter, translatedcalendarEventParameter);
            var translatedBody = visitor.Visit(predicate.Body);

            Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, bool>> translated = calendarEvent => true;
            translated = translated.Update(translatedBody, new[] { translated.Parameters[0] }); //// new[] { translatedcalendarEventParameter });

            var lambda = Expression.Lambda<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, bool>>(translatedBody, translatedcalendarEventParameter);
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
                this.ApplicableToSeriesMaster = false;
            }

            public bool ApplicableToSeriesMaster { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return this.translatedCalendarEventExpression;

                /*if (object.ReferenceEquals(node, this.originalCalendarEventParamter))
                {
                    return this.translatedCalendarEventExpression;
                }

                return base.VisitParameter(node);*/
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (object.ReferenceEquals(node.Expression, this.originalCalendarEventParamter))
                {
                    if (ExpressionVisitor.SeriesMasterAdapters.TryGetValue(node.Member.Name, out var seriesMasterExpression))
                    {
                        seriesMasterExpression = this.Visit(seriesMasterExpression);

                        this.ApplicableToSeriesMaster = true;
                        return seriesMasterExpression;
                    }

                    if (ExpressionVisitor.CalendarEventAdapters.TryGetValue(node.Member.Name, out var calendarEventExpression))
                    {
                        calendarEventExpression = this.Visit(calendarEventExpression);

                        return calendarEventExpression;
                    }
                }

                return base.VisitMember(node);
            }

            private static Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, string>> SubjectExpression { get; } = calendarEvent => calendarEvent.Subject;
            private static Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, string>> IdExpression { get; } = calendarEvent => calendarEvent.Id;
            private static Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, string>> BodyExpression { get; } = calendarEvent => calendarEvent.Body.Content;
            private static Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, bool>> IsCancelledExpression { get; } = calendarEvent => calendarEvent.IsCancelled;
            private static Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, string>> TypeExpression { get; } = calendarEvent => calendarEvent.Type;

            private static IReadOnlyDictionary<string, Expression> SeriesMasterAdapters { get; } = new Dictionary<string, Expression>()
            {
                { "Subject", ExpressionVisitor.SubjectExpression.Body },
                { "Id", ExpressionVisitor.IdExpression.Body },
                { "Body", ExpressionVisitor.BodyExpression.Body },
                { "IsCancelled", ExpressionVisitor.IsCancelledExpression.Body },
                { "Type", ExpressionVisitor.TypeExpression.Body },
            };

            private static Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, DateTime>> StartExpression { get; } = calendarEvent => calendarEvent.Start.DateTime;
            private static Expression<Func<OddTrotter.Graph.CalendarEventsContext.CalendarEvent, DateTime>> EndExpression { get; } = calendarEvent => calendarEvent.End.DateTime;

            private static IReadOnlyDictionary<string, Expression> CalendarEventAdapters { get; } = new Dictionary<string, Expression>()
            {
                { "Start", ExpressionVisitor.StartExpression.Body },
                { "End", ExpressionVisitor.EndExpression.Body },
            };
        }






        [TestMethod]
        public void EitherFactoryTest()
        {
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
}
