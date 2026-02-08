namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System;
    using System.Collections.Generic;

    using Fx.Either;

    using OddTrotter.Odata.v4_01.ProtocolContext;

    internal abstract class GetCollectionResponse<T>
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

        internal sealed class Success : GetCollectionResponse<T>
        {
            public Success(IEnumerable<CollectionElement<T>> elements, string? nextLink)
            {
                this.Elements = elements;
                NextLink = nextLink;
            }

            internal IEnumerable<CollectionElement<T>> Elements { get; }

            internal string? NextLink { get; }
        }

        internal sealed class Failure : GetCollectionResponse<T>
        {
        }
    }

    internal sealed class CollectionElement<T>
    {
        public CollectionElement(IEither<T, DeserializationError> element)
        {
            Element = element;
        }
        
        internal IEither<T, DeserializationError> Element { get; }
    }

    internal sealed class DeserializationError
    {
        public DeserializationError(DeserializationException exception, OdataObject odataObject)
        {
            Exception = exception;
            OdataObject = odataObject; //// TODO is this leaking? do you need to wrap it in a "strong convention context" type?
        }

        public DeserializationException Exception { get; }
        public OdataObject OdataObject { get; }
    }


    //// TODO it'd be really cool still to have a "context" type that mimics the structure of the edm model, and is strongly typed with things like `structuralproperty<T>` and `navigationproperty<T>`
}
