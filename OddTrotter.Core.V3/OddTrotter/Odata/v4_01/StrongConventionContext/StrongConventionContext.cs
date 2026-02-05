namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Fx.Either;

    using OddTrotter.Odata.v4_01.WeakConventionContext;

    using WeakConventionContext = OddTrotter.Odata.v4_01.WeakConventionContext;

    internal sealed class StrongConventionContext<T> : IStrongConventionContext<T>
    {
        private readonly IWeakConventionContext weakConventionContext;
        private readonly IDeserializer<T> deserializer;

        public StrongConventionContext(IWeakConventionContext weakConventionContext, IDeserializer<T> deserializer)
        {
            this.weakConventionContext = weakConventionContext;
            this.deserializer = deserializer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="WriteException">Thrown if an error occurred writing to the underlying stream</exception>
        /// <exception cref="HttpRequestException">Thrown if an error occurred trasmitting data between the client and the service</exception> //// TODO do you want to split this into 2 exceptions, one for read and one for write? //// TODO i'm not sure you can always differentiate, and if you can, i'm not sure there is an actionable difference
        /// <exception cref="ReadException">Thrown if an error occurred reading from the underlying stream</exception>
        /// <exception cref="StrongConventionException">Thrown if the underlying response payload is not valid OData or does not represent a collection response</exception>
        public async Task<GetCollectionResponse<T>> GetCollection(GetCollectionRequest<T> request)
        {
            var weakConventionRequest = new WeakConventionContext.GetCollectionRequest(
                request.Url,
                request.Headers);
            WeakConventionContext.GetCollectionResponse weakConventionResponse;
            try
            {
                weakConventionResponse = await this.weakConventionContext.GetCollection(weakConventionRequest).ConfigureAwait(false);
            }
            catch (WeakConventionContext.ReadException readException)
            {
                throw new ReadException("TODO", readException);
            }
            catch (WeakConventionContext.WriteException writeException)
            {
                throw new WriteException("TODO", writeException);
            }
            catch (WeakConventionException weakConventionException)
            {
                throw new StrongConventionException("TODO", weakConventionException);
            }

            return weakConventionResponse.Apply<GetCollectionResponse<T>>(
                success =>
                {
                    return new GetCollectionResponse<T>.Success(
                        success
                            .Value
                            .Elements
                            .Select(
                                element =>
                                {
                                    T deserialized;
                                    try
                                    {
                                        deserialized = this.deserializer.Deserialize(element.OdataObject);
                                    }
                                    catch (DeserializationException deserializationException)
                                    {
                                        return new CollectionElement<T>(
                                            Either
                                                .Left<T>()
                                                .Right(
                                                    new DeserializationError(
                                                        deserializationException,
                                                        element.OdataObject)));
                                    }

                                    return new CollectionElement<T>(
                                        Either.Right<DeserializationError>().Left(deserialized));
                                }));
                },
                failure => new GetCollectionResponse<T>.Failure());
        }
    }
}
