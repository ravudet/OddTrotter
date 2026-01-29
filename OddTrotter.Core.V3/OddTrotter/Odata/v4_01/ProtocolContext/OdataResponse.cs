namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal sealed class OdataResponse
    {
        public OdataResponse(string httpVerb, IEnumerable<HttpHeader> headers, IEnumerable<OdataProperty> properties)
        {
            HttpVerb = httpVerb;
            Headers = headers;
            Properties = properties;
        }

        public string HttpVerb { get; }
        public IEnumerable<HttpHeader> Headers { get; }
        public IEnumerable<OdataProperty> Properties { get; }
    }

    internal sealed class OdataProperty
    {
        public OdataProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
    }
}
