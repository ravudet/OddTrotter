namespace OddTrotter.Odata.v4_01.Reader.ResponseReader
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    internal sealed class ResponseReader : IResponseReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        internal ResponseReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public IStatusCodeReader Read()
        {
            return new StatusCodeReader(this.httpResponseMessage);
        }
    }

    internal sealed class StatusCodeReader : IStatusCodeReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        internal StatusCodeReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public IHeadersReader Read(out HttpStatusCode httpStatusCode)
        {
            httpStatusCode = new HttpStatusCode(this.httpResponseMessage.StatusCode.ToString());
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

        public async Task<HeadersToken> Read()
        {
            if (!this.headers.MoveNext())
            {
                return new HeadersToken.Body(new BodyReader(await this.httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false), 0));
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
        private readonly byte[] bytes;
        private readonly long index;

        internal BodyReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public BodyToken Read()
        {
            if (this.index == this.bytes.Length)
            {
                return BodyToken.End.Instance;
            }

            return new BodyToken.Property(new PropertyReader(this.bytes, this.index));
        }
    }

    internal sealed class PropertyReader : IPropertyReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal PropertyReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public IPropertyNameReader Read()
        {
            return new PropertyNameReader(this.bytes, this.index);
        }
    }

    internal sealed class PropertyNameReader : IPropertyNameReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal PropertyNameReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public IPropertyValueReader Read(out PropertyName propertyName)
        {
            long i;
            var slicedBytes = this.bytes.AsSpan();
            for (i = this.index; i > int.MaxValue; i -= int.MaxValue)
            {
                slicedBytes = slicedBytes.Slice(int.MaxValue);
            }

            var jsonReader = new Utf8JsonReader(slicedBytes.Slice((int)i));
            if (!jsonReader.Read())
            {
                throw new OdataException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            if (jsonReader.TokenType != JsonTokenType.PropertyName)
            {
                throw new OdataException("TODO");
            }

            var receivedPropertyName = jsonReader.GetString();
            if (string.IsNullOrEmpty(receivedPropertyName))
            {
                throw new OdataException("TODO");
            }

            propertyName = new PropertyName(receivedPropertyName);
            return new PropertyValueReader(this.bytes, jsonReader.BytesConsumed);
        }
    }

    internal sealed class PropertyValueReader : IPropertyValueReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal PropertyValueReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public PropertyValueToken Read()
        {
            long i;
            var slicedBytes = this.bytes.AsSpan();
            for (i = this.index; i > int.MaxValue; i -= int.MaxValue)
            {
                slicedBytes = slicedBytes.Slice(int.MaxValue);
            }

            var jsonReader = new Utf8JsonReader(slicedBytes.Slice((int)i));
            if (!jsonReader.Read())
            {
                throw new OdataException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            //// TODO you need to always skip comments in the JSON
            switch (jsonReader.TokenType)
            {
                case JsonTokenType.False:
                case JsonTokenType.Number:
                case JsonTokenType.True:
                    return new PropertyValueToken.Literal(new LiteralReader(this.bytes, this.index));
                case JsonTokenType.Null:
                    return new PropertyValueToken.Null(new NullReader(this.bytes, this.index));
                case JsonTokenType.String:
                    return new PropertyValueToken.String(new StringReader(this.bytes, this.index));
                default:
                    throw new OdataException("TODO");
            }
        }
    }

    internal sealed class LiteralReader : ILiteralReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal LiteralReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public LiteralToken Read()
        {
            long i;
            var slicedBytes = this.bytes.AsSpan();
            for (i = this.index; i > int.MaxValue; i -= int.MaxValue)
            {
                slicedBytes = slicedBytes.Slice(int.MaxValue);
            }

            var jsonReader = new Utf8JsonReader(slicedBytes.Slice((int)i));
            if (!jsonReader.Read())
            {
                throw new OdataException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            switch (jsonReader.TokenType)
            {
                case JsonTokenType.False:
                    return new LiteralToken.False(new FalseReader(this.bytes, this.index));
                case JsonTokenType.Number:
                    return new LiteralToken.Number(new NumberReader(this.bytes, this.index));
                case JsonTokenType.True:
                    return new LiteralToken.True(new TrueReader(this.bytes, this.index));
                default:
                    throw new OdataException("TODO");
            }
        }
    }

    internal sealed class FalseReader : IFalseReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal FalseReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public IBodyReader Read(out FalseToken falseToken)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class NumberReader : INumberReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal NumberReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public IBodyReader Read(out Number number)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class TrueReader : ITrueReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal TrueReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public IBodyReader Read(out TrueToken trueToken)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class NullReader : INullReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal NullReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public IBodyReader Read(out NullToken nullToken)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class StringReader : IStringReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        internal StringReader(byte[] bytes, long index)
        {
            this.bytes = bytes;
            this.index = index;
        }

        public IBodyReader Read(out StringToken stringToken)
        {
            throw new NotImplementedException();
        }
    }
}
