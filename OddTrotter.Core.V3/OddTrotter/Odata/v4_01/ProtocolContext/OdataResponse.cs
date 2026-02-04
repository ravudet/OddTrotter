namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal sealed class OdataResponse
    {
        public OdataResponse(string httpStatusCode, IEnumerable<HttpHeader> headers, IEnumerable<OdataProperty> properties)
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
            Properties = properties;
        }

        public string HttpStatusCode { get; } //// TODO this should be strongly typed
        public IEnumerable<HttpHeader> Headers { get; } //// TODO make sure `httpheader` properly validates; another option would be to make it very strongly typed //// TODO actually, should there be "odataheader"s? this wouldn't just be headers specific to odata (like odataversion), but also headers that odata takes a strong opinion on (like accept headers)
        public IEnumerable<OdataProperty> Properties { get; }
    }

    internal sealed class OdataProperty
    {
        public OdataProperty(string name, string value)
        {
            //// TODO this needs to either validate `name` and `value`, or it needs to make them strongly typed

            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
    }
}
