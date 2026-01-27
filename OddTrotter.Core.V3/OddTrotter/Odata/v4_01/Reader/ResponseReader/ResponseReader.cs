namespace OddTrotter.Odata.v4_01.Reader.ResponseReader
{
    using System.Collections.Generic;
    using System.Net.Http;

    internal sealed class ResponseReader : IResponseReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        internal ResponseReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public IHeadersReader Read()
        {
            return new HeadersReader(this.httpResponseMessage, this.httpResponseMessage.Headers.GetEnumerator());
        }
    }

    internal sealed class HeadersReader : IHeadersReader
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;

        internal HeadersReader(
            HttpResponseMessage httpResponseMessage, 
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
        }

        public HeadersToken Read()
        {
            if (!this.headers.MoveNext())
            {
                return new HeadersToken.Body(new BodyReader(this.httpResponseMessage));
            }
            else
            {
                return new HeadersToken.Header(new HeaderReader(this.httpResponseMessage, this.headers));
            }
        }
    }

    internal sealed class HeaderReader : IHeaderReader
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;

        internal HeaderReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
        }

        public IHeaderKvpReader Read()
        {
            return new HeaderKvpReader(this.httpResponseMessage, this.headers);
        }
    }

    internal sealed class HeaderKvpReader : IHeaderKvpReader
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;

        internal HeaderKvpReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
        }

        public IHeaderKeyReader Read()
        {
            return new HeaderKeyReader(this.httpResponseMessage, this.headers);
        }
    }

    internal sealed class HeaderKeyReader : IHeaderKeyReader
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;

        internal HeaderKeyReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
        }

        public HeaderKeyToken Read(out HeaderKey headerKey)
        {
            var header = this.headers.Current;
            headerKey = new HeaderKey(header.Key);

            var values = header.Value.GetEnumerator();
            if (!values.MoveNext())
            {
                return new HeaderKeyToken.Headers(new HeadersReader(this.httpResponseMessage, this.headers));
            }
            else
            {
                return new HeaderKeyToken.HeaderValue(new HeaderValueReader(this.httpResponseMessage, this.headers, values));
            }
        }
    }

    internal sealed class HeaderValueReader : IHeaderValueReader
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;
        private readonly IEnumerator<string> values;

        internal HeaderValueReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers,
            IEnumerator<string> values)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
            this.values = values;
        }

        public HeaderValueToken Read(out HeaderValue headerValue)
        {
            headerValue = new HeaderValue(this.values.Current);
            if (!this.values.MoveNext())
            {
                return new HeaderValueToken.Headers(new HeadersReader(this.httpResponseMessage, this.headers));
            }
            else
            {
                return new HeaderValueToken.HeaderValue(new HeaderValueReader(this.httpResponseMessage, this.headers, this.values));
            }
        }
    }

    internal sealed class BodyReader : IBodyReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        internal BodyReader(
            HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public BodyToken Read()
        {
            //// TODO actually implement this

            return BodyToken.End.Instance;
        }
    }
}
