namespace OddTrotter.Odata.v4_01.StrongConventionContext
{
    using OddTrotter.Odata.v4_01.ProtocolContext;

    internal interface IDeserializer<out T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="odataObject"></param>
        /// <returns></returns>
        /// <exception cref="DeserializationException"></exception>
        T Deserialize(OdataObject odataObject);
    }
}
