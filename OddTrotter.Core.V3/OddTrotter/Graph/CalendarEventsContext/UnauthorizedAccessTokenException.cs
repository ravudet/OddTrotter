namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;

    public sealed class UnauthorizedAccessTokenException : Exception
    {
        public UnauthorizedAccessTokenException(string url, string accessToken, string message)
            : base(message)
        {
            Url = url;
            AccessToken = accessToken;
        }

        public string Url { get; }

        public string AccessToken { get; }
    }
}
