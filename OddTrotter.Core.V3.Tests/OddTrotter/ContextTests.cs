namespace OddTrotter
{
    using System;
    using System.Net.Http;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OddTrotter.Calendar;
    using OddTrotter.Graph.CalendarEventsContext;
    using OddTrotter.Graph.CalendarEventsSource;
    using OddTrotter.Odata.v4_01.ProtocolContext;
    using OddTrotter.Odata.v4_01.Reader.RequestReader;
    using OddTrotter.Odata.v4_01.Reader.RequestWriter;
    using OddTrotter.Odata.v4_01.StrongConventionContext;
    using OddTrotter.Odata.v4_01.WeakConventionContext;
    using OddTrotter.TodoListService;

    [TestClass]
    public sealed class ContextTests
    {
        [TestMethod]
        public void Run()
        {
            var requestReaderFactory = (HttpRequestMessage httpRequestMessage) => new RequestReader(httpRequestMessage);
            using (var httpClient = new HttpClient())
            {
                var requestWriterFactory = () => new RequestWriter(new HttpClientAdapter(httpClient));
                var protocolContext = new ProtocolContext(requestReaderFactory, requestWriterFactory);
                var weakConventionContext = new WeakConventionContext(protocolContext, StringComparer.Ordinal);
                var propertyNameComparer = StringComparer.OrdinalIgnoreCase;
                var calendarEventDeserializer = new CalendarEventDeserializer(
                    new BodyStructureDeserializer(propertyNameComparer), 
                    new TimeStructureDeserializer(propertyNameComparer), 
                    propertyNameComparer);
                var strongConventionContext = new StrongConventionContext<Graph.CalendarEventsContext.CalendarEvent>(
                    weakConventionContext,
                    calendarEventDeserializer);
                var accessToken = "TODO";
                var calendarSource = new CalendarSource(
                    strongConventionContext,
                    new Uri("https://graph.microsoft.com/v1.0/me/calendar"),
                    accessToken);
                var calendarEventsContext = new OddTrotter.CalendarEventsContext.CalendarEventsContext(calendarSource, DateTime.UtcNow); //// TODO parameterize the timestamp
                var todoList = new TodoListService<OddTrotter.CalendarEventsContext.CalendarEventsContext>(calendarEventsContext);


            }
        }
    }
}
