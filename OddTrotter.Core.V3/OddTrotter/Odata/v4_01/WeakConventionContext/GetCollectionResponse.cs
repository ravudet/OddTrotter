namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System;
    using System.Collections.Generic;

    using OddTrotter.Calendar;
    using OddTrotter.Odata.v4_01.ProtocolContext;

    internal abstract class GetCollectionResponse
    {
        private GetCollectionResponse()
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

        internal sealed class Success : GetCollectionResponse
        {
            public Success(OddTrotter.Odata.v4_01.WeakConventionContext.Success value)
            {
                Value = value;
            }

            public v4_01.WeakConventionContext.Success Value { get; }
        }

        internal sealed class Failure : GetCollectionResponse
        {
        }
    }

    internal sealed class Success
    {
        internal Success(string httpStatusCode, IEnumerable<HttpHeader> headers, IEnumerable<CollectionElement> elements, string? nextLink)
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
            Elements = elements;
            NextLink = nextLink;
        }

        public string HttpStatusCode { get; }
        public IEnumerable<HttpHeader> Headers { get; }
        public IEnumerable<CollectionElement> Elements { get; }
        public string? NextLink { get; } //// TODO i don't think the nextlink is ever actually allowed to be `null`, it's either present or it isn't, so maybe represent this differently here
    }

    internal sealed class CollectionElement
    {
        internal CollectionElement(OdataObject odataObject)
        {
            OdataObject = odataObject;
        }

        public OdataObject OdataObject { get; }
    }
}
