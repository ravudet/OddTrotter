namespace OddTrotter.Odata.v4_01.ProtocolContext
{
    internal sealed class OdataRequest
    {
        private OdataRequest(string httpVerb, string url)
        {
            HttpVerb = httpVerb;
            Url = url;
        }

        internal string HttpVerb { get; }
        
        internal string Url { get; }
    }
}
