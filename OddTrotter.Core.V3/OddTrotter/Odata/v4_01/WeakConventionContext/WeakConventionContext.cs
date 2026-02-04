namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.AccessControl;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.ProtocolContext;

    internal sealed class WeakConventionContext : IWeakConventionContext
    {
        private readonly IProtocolContext protocolContext;
        private readonly IEqualityComparer<string> propertyNameComparer;

        public WeakConventionContext(IProtocolContext protocolContext, IEqualityComparer<string> propertyNameComparer)
        {
            this.protocolContext = protocolContext;
            this.propertyNameComparer = propertyNameComparer;
        }

        public async Task<GetCollectionResponse> GetCollection(GetCollectionRequest request)
        {
            var odataRequest = new OdataRequest(
                "GET",
                request.Url,
                request.Headers);
            var odataResponse = await this.protocolContext.Send(odataRequest).ConfigureAwait(false);

            if (!odataResponse.Properties.Where(property => this.propertyNameComparer.Equals(property.Name, "value")).TrySingle(out var valueProperty))
            {
                //// TODO should there be two error messages, one for "value wasn't present" and another for "other properties are present"?
                throw new Exception("TODO");
            }

            if (!(valueProperty.Value is OdataPropertyValue.Collection collection))
            {
                throw new Exception("TODO");
            }

            //// TODO this *maybe* should still have a status code property, but errors are supposed to be in the `odataerror` format //// TODO this maybe even should be done at the reader level...
            return new GetCollectionResponse(
                odataResponse.HttpStatusCode,
                odataResponse.Headers,
                collection.Elements.Select(
                    element => new CollectionElement(element)));
        }
    }

    internal static class Extensions
    {
        public static bool TrySingle<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
        {
            try
            {
                value = source.Single();
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }
    }
}
