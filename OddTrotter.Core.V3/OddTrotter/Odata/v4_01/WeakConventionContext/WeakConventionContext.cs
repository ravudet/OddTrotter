namespace OddTrotter.Odata.v4_01.WeakConventionContext
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Protocol = OddTrotter.Odata.v4_01.ProtocolContext;

    internal sealed class WeakConventionContext : IWeakConventionContext
    {
        private readonly Protocol.IProtocolContext protocolContext;
        private readonly IEqualityComparer<string> propertyNameComparer;

        public WeakConventionContext(
            Protocol.IProtocolContext protocolContext, 
            IEqualityComparer<string> propertyNameComparer)
        {
            this.protocolContext = protocolContext;
            this.propertyNameComparer = propertyNameComparer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception> //// TODO do you want to split this into 2 exceptions, one for read and one for write? //// TODO i'm not sure you can always differentiate, and if you can, i'm not sure there is an actionable difference
        /// <exception cref="ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="WeakConventionContext">Thrown if the underlying response payload is not valid OData or does not represent a collection response</exception>
        public async Task<GetCollectionResponse> GetCollection(GetCollectionRequest request)
        {
            var odataRequest = new Protocol.OdataRequest(
                "GET",
                request.Url,
                request.Headers);
            Protocol.OdataResponse odataResponse;
            try
            {
                odataResponse = await this.protocolContext.Send(odataRequest).ConfigureAwait(false);
            }
            catch (Protocol.ReadException readException)
            {
                throw new ReadException("TODO", readException);
            }
            catch (Protocol.WriteException writeException)
            {
                throw new WriteException("TODO", writeException);
            }
            catch (Protocol.ProtocolException protocolException)
            {
                throw new WeakConventionException("TODO", protocolException);
            }

            return odataResponse.Apply<GetCollectionResponse>(
                success =>
                {
                    if (!success.Value.Properties.Where(property => this.propertyNameComparer.Equals(property.Name, "value")).TrySingle(out var valueProperty))
                    {
                        //// TODO should there be two error messages, one for "value wasn't present" and another for "other properties are present"?
                        throw new WeakConventionException("TODO");
                    }

                    if (!(valueProperty.Value is Protocol.OdataPropertyValue.Collection collection))
                    {
                        throw new WeakConventionException("TODO");
                    }

                    return new GetCollectionResponse.Success(
                        new Success(
                            success.Value.HttpStatusCode,
                            success.Value.Headers,
                            collection.Elements.Select(
                                element => new CollectionElement(element))));
                },
                failure => new GetCollectionResponse.Failure());
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
