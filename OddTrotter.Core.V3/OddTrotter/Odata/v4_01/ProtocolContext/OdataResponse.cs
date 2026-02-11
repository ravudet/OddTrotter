namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    using System;
    using System.Collections.Generic;

    using OddTrotter.Calendar;

    internal abstract class OdataResponse
    {
        private OdataResponse()
        {
        }

        internal TResult Apply<TResult>(
            Func<Success, TResult> successMap,
            Func<Failure, TResult> failureMap)
        {
            if (this is Success success)
            {
                return successMap(success);
            }
            else if (this is Failure failure)
            {
                return failureMap(failure);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class Success : OdataResponse
        {
            public Success(OddTrotter.Odata.v4_01.ProtocolContext.Success value)
            {
                Value = value;
            }

            public v4_01.ProtocolContext.Success Value { get; }
        }

        internal sealed class Failure : OdataResponse
        { 
        }
    }

    internal sealed class Success
    {
        public Success(
            string httpStatusCode,
            IEnumerable<HttpHeader> headers, 
            IEnumerable<OdataProperty> properties, 
            IEnumerable<ControlInformation> controlInformation)
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
            Properties = properties;
            this.ControlInformation = controlInformation;
        }

        public string HttpStatusCode { get; } //// TODO this should be strongly typed (and probably at reader level too); also, this should only allow *success* status codes

        public IEnumerable<HttpHeader> Headers { get; } //// TODO make sure `httpheader` properly validates; another option would be to make it very strongly typed //// TODO actually, should there be "odataheader"s? this wouldn't just be headers specific to odata (like odataversion), but also headers that odata takes a strong opinion on (like accept headers)

        public IEnumerable<OdataProperty> Properties { get; }

        public IEnumerable<ControlInformation> ControlInformation { get; }
    }

    internal abstract class ControlInformation
    {
        private ControlInformation()
        {
        }

        internal sealed class NextLink : ControlInformation
        {
            public NextLink(string url)
            {
                Url = url;
            }

            public string Url { get; }
        }

        internal sealed class Unknown : ControlInformation
        {
            public Unknown(string name, string value)
            {
                Name = name;
                Value = value;
                // you need `unknown` because odata asks that you skip control information that you don't understand, but we still want that information to be available to the caller
            }

            public string Name { get; }
            public string Value { get; }
        }
    }

    internal sealed class OdataProperty
    {
        public OdataProperty(string name, OdataPropertyValue value)
        {
            //// TODO this needs to either validate `name` and `value`, or it needs to make them strongly typed

            Name = name;
            Value = value;
        }

        public string Name { get; }
        public OdataPropertyValue Value { get; }
    }

    internal abstract class OdataPropertyValue
    {
        private OdataPropertyValue()
        {
        }

        internal TResult Apply<TResult>(
            Func<OdataPropertyValue.String, TResult> stringMap,
            Func<OdataPropertyValue.Object, TResult> objectMap,
            Func<OdataPropertyValue.Collection, TResult> collectionMap)
        {
            if (this is OdataPropertyValue.String @string)
            {
                return stringMap(@string);
            }
            else if (this is OdataPropertyValue.Object @object)
            {
                return objectMap(@object);
            }
            else if (this is OdataPropertyValue.Collection collection)
            {
                return collectionMap(collection);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        public sealed class String : OdataPropertyValue
        {
            public String(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public sealed class Object : OdataPropertyValue
        {
        }

        public sealed class Collection : OdataPropertyValue
        {
            public Collection(IReadOnlyList<OdataObject> elements)
            {
                Elements = elements;
            }

            public IReadOnlyList<OdataObject> Elements { get; }
        }
    }

    internal sealed class OdataObject
    {
        public OdataObject(
            IEnumerable<OdataProperty> properties,
            IEnumerable<ControlInformation> controlInformation)
        {
            Properties = properties;
            ControlInformation = controlInformation;
        }

        public IEnumerable<OdataProperty> Properties { get; }
        public IEnumerable<ControlInformation> ControlInformation { get; }
    }
}
