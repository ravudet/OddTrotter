namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal sealed class OdataResponse
    {
        public OdataResponse(IEnumerable<HttpHeader> headers, IEnumerable<Property> properties)
        {
            Headers = headers;
            Properties = properties;
        }

        public IEnumerable<HttpHeader> Headers { get; }
        public IEnumerable<Property> Properties { get; }
    }

    internal sealed class Property
    {
        public Property(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
    }
}
