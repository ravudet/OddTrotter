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
            //// TODO document somewhere in odata framework stuff your guiding principle that the interfaces should be the same for the client developer as for the service developer; this guarantees that both parties have the same understanding of the contract
            //// TODO in icalendareventssource, you have a thing where you are trying to create the interfaces to pretend that there is a service between oddtrotter and graph; this would really prove the above design principle, because oddtrotter itself could have `oddtrotter.calendareventscontext` take in a client to call the intermediate service, or it could do that translation work locally; and it does this by taking the "adapted" interface in either case, so the "oddtrotter" functionality remains agnostic to the where the data is coming from

            var requestReaderFactory = (HttpRequestMessage httpRequestMessage) => new RequestReader(httpRequestMessage);
            using (var httpClient = new HttpClient())
            {
                var httpClientAdapter = new HttpClientAdapter(httpClient);
                var accessToken = "TODO";
                var requestWriterFactory = () => new AuthorizedRequestWriter(new RequestWriter(httpClientAdapter), accessToken); //// TODO it might be nifty to have an extension like `.Authorize(accessToken)`
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
                var calendarSource = new CalendarSource(
                    strongConventionContext,
                    new Uri("https://graph.microsoft.com/v1.0/me/calendar"));
                var todoList = new TodoListService.TodoListService(calendarSource , CalendarEventsContext.CalendarEventsContextSettings.Default);


            }
        }
    }
}
