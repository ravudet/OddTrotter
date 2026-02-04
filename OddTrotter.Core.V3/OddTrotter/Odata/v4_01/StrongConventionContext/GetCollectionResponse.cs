namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System;
    using System.Collections.Generic;

    using Fx.Either;

    internal sealed class GetCollectionResponse<T>
    {
        private GetCollectionResponse(IEnumerable<IEither<T, Exception>> elements)
        {
            this.Elements = elements;
        }

        internal IEnumerable<IEither<T, Exception>> Elements { get; }
    }
}
