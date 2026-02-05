namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using System.Linq;
    using System.Threading.Tasks;

    using Fx.Either;
    using OddTrotter.Odata.v4_01.WeakConventionContext;

    internal sealed class StrongConventionContext<T> : IStrongConventionContext<T>
    {
        private readonly IWeakConventionContext weakConventionContext;
        private readonly IDeserializer<T> deserializer;

        public StrongConventionContext(IWeakConventionContext weakConventionContext, IDeserializer<T> deserializer)
        {
            this.weakConventionContext = weakConventionContext;
            this.deserializer = deserializer;
        }

        public async Task<GetCollectionResponse<T>> GetCollection(GetCollectionRequest<T> request)
        {
            //// TODO write down the exceptions
            var weakConventionRequest = new GetCollectionRequest(
                request.Url,
                request.Headers);
            var weakConventionResponse = await this.weakConventionContext.GetCollection(weakConventionRequest).ConfigureAwait(false);

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
