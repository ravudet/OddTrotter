namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System;
    using System.Collections.Generic;

    using Fx.Either;

    internal sealed class GetCollectionResponse<T>
    {
        private GetCollectionResponse(IEnumerable<CollectionElement<T>> elements)
        {
            this.Elements = elements;
        }

        internal IEnumerable<CollectionElement<T>> Elements { get; }
    }

    internal sealed class CollectionElement<T>
    {
        public CollectionElement(IEither<T, Exception> element)
        {
            Element = element;
        }

        //// TODO annotations and stuff go here

        internal IEither<T, Exception> Element { get; }
    }
}
