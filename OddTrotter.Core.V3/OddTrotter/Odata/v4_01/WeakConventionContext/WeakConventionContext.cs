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

            if (!odataResponse.Properties.Where(property => this.propertyNameComparer.Equals(property.Name, "value")).TrySingle(out var value))
            {
                throw new Exception("TODO");
            }
            
            return new GetCollectionResponse();
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
