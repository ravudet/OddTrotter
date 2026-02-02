namespace OddTrotter.Odata.v4_01.Reader.ResponseReader
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    internal sealed class ResponseReader : IResponseReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        internal ResponseReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public async Task<IStatusCodeReader> Read()
        {
            return await Task.FromResult(new StatusCodeReader(this.httpResponseMessage)).ConfigureAwait(false);
        }
    }

    internal sealed class StatusCodeReader : IStatusCodeReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        internal StatusCodeReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public async Task<(IHeadersReader HeadersReader, HttpStatusCode HttpStatusCode)> Read()
        {
            var httpStatusCode = new HttpStatusCode(this.httpResponseMessage.StatusCode.ToString());
            var headersReader = new HeadersReader(this.httpResponseMessage, this.httpResponseMessage.Headers.GetEnumerator());

            return await Task.FromResult((headersReader,  httpStatusCode)).ConfigureAwait(false);
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

        public async Task<IHeaderKvpReader> Read()
        {
            return await Task.FromResult(new HeaderKvpReader(this.httpResponseMessage, this.headers)).ConfigureAwait(false);
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

        public async Task<IHeaderKeyReader> Read()
        {
            return await Task.FromResult(new HeaderKeyReader(this.httpResponseMessage, this.headers)).ConfigureAwait(false);
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

        public async Task<(HeaderKeyToken HeaderKeyToken, HeaderKey HeaderKey)> Read()
        {
            var header = this.headers.Current;
            var headerKey = new HeaderKey(header.Key);

            var values = header.Value.GetEnumerator();
            if (!values.MoveNext())
            {
                var token = new HeaderKeyToken.Headers(new HeadersReader(this.httpResponseMessage, this.headers));

                return await Task.FromResult((token, headerKey)).ConfigureAwait(false);
            }
            else
            {
                var token = new HeaderKeyToken.HeaderValue(new HeaderValueReader(this.httpResponseMessage, this.headers, values));

                return await Task.FromResult((token, headerKey)).ConfigureAwait(false);
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

        public async Task<(HeaderValueToken HeaderValueToken, HeaderValue HeaderValue)> Read()
        {
            var headerValue = new HeaderValue(this.values.Current);
            if (!this.values.MoveNext())
            {
                var token = new HeaderValueToken.Headers(new HeadersReader(this.httpResponseMessage, this.headers));

                return await Task.FromResult((token, headerValue)).ConfigureAwait(false);
            }
            else
            {
                var token = new HeaderValueToken.HeaderValue(new HeaderValueReader(this.httpResponseMessage, this.headers, this.values));

                return await Task.FromResult((token, headerValue)).ConfigureAwait(false);
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

        public async Task<BodyToken> Read()
        {
            if (this.index == this.bytes.Length)
            {
                return await Task.FromResult(BodyToken.End.Instance).ConfigureAwait(false);
            }

            return await Task.FromResult(new BodyToken.Property(new PropertyReader(this.bytes, this.index))).ConfigureAwait(false);
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

        public async Task<IPropertyNameReader> Read()
        {
            return await Task.FromResult(new PropertyNameReader(this.bytes, this.index)).ConfigureAwait(false);
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

        public async Task<(IPropertyValueReader PropertyValueReader, PropertyName PropertyName)> Read()
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

            var propertyName = new PropertyName(receivedPropertyName);
            var propertyValueReader = new PropertyValueReader(this.bytes, jsonReader.BytesConsumed);

            return await Task.FromResult((propertyValueReader, propertyName)).ConfigureAwait(false);
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

        public async Task<PropertyValueToken> Read()
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
                    return await Task.FromResult(new PropertyValueToken.Literal(new LiteralReader(this.bytes, this.index))).ConfigureAwait(false);
                case JsonTokenType.Null:
                    return await Task.FromResult(new PropertyValueToken.Null(new NullReader(this.bytes, this.index))).ConfigureAwait(false);
                case JsonTokenType.String:
                    return await Task.FromResult(new PropertyValueToken.String(new StringReader(this.bytes, this.index))).ConfigureAwait(false);
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

        public async Task<LiteralToken> Read()
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
                    return await Task.FromResult(new LiteralToken.False(new FalseReader(this.bytes, this.index))).ConfigureAwait(false);
                case JsonTokenType.Number:
                    return await Task.FromResult(new LiteralToken.Number(new NumberReader(this.bytes, this.index))).ConfigureAwait(false);
                case JsonTokenType.True:
                    return await Task.FromResult(new LiteralToken.True(new TrueReader(this.bytes, this.index))).ConfigureAwait(false);
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

        public async Task<(IBodyReader BodyReader, FalseToken FalseToken)> Read()
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

            if (jsonReader.TokenType != JsonTokenType.False)
            {
                throw new OdataException("TODO");
            }

            var falseToken = FalseToken.Instance;
            var bodyReader = new BodyReader(this.bytes, jsonReader.BytesConsumed);

            return await Task.FromResult((bodyReader, falseToken)).ConfigureAwait(false);
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

        public async Task<(IBodyReader BodyReader, Number Number)> Read()
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

            if (jsonReader.TokenType != JsonTokenType.Number)
            {
                throw new OdataException("TODO");
            }

            var number = new Number(Encoding.UTF8.GetString(jsonReader.ValueSpan));
            var bodyReader = new BodyReader(this.bytes, jsonReader.BytesConsumed);

            return await Task.FromResult((bodyReader, number)).ConfigureAwait(false);
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

        public async Task<(IBodyReader BodyReader, TrueToken TrueToken)> Read()
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

            if (jsonReader.TokenType != JsonTokenType.True)
            {
                throw new OdataException("TODO");
            }

            var trueToken = TrueToken.Instance;
            var bodyReader = new BodyReader(this.bytes, jsonReader.BytesConsumed);

            return await Task.FromResult((bodyReader, trueToken)).ConfigureAwait(false);
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

        public async Task<(IBodyReader BodyReader, NullToken NullToken)> Read()
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

            if (jsonReader.TokenType != JsonTokenType.Null)
            {
                throw new OdataException("TODO");
            }

            var nullToken = NullToken.Instance;
            var bodyReader = new BodyReader(this.bytes, jsonReader.BytesConsumed);

            return await Task.FromResult((bodyReader, nullToken)).ConfigureAwait(false);
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

        public async Task<(IBodyReader BodyReader, StringToken StringToken)> Read()
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

            if (jsonReader.TokenType != JsonTokenType.String)
            {
                throw new OdataException("TODO");
            }

            var receivedPropertyName = jsonReader.GetString();
            if (receivedPropertyName == null)
            {
                throw new OdataException("TODO what would a null value even mean here? is this just a jsonreader deficiency?");
            }

            var stringToken = new StringToken(receivedPropertyName);
            var bodyReader = new BodyReader(this.bytes, jsonReader.BytesConsumed);

            return await Task.FromResult((bodyReader, stringToken)).ConfigureAwait(false);
        }
    }
}
