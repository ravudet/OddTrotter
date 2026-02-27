namespace OddTrotter.Odata.v4_01.Reader.ResponseReader
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader.RequestReader;

    internal sealed class ResponseReader : IResponseReader
    {
        private readonly HttpResponseMessage httpResponseMessage;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpResponseMessage"/> is <see langword="null"/></exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpResponseMessage"/> is <see langword="null"/></exception>
        internal StatusCodeReader(HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage;
        }

        public async Task<(StatusCodeToken StatusCodeToken, HttpStatusCode HttpStatusCode)> Read()
        {
            var httpStatusCode = new HttpStatusCode(this.httpResponseMessage.StatusCode.ToString());
            StatusCodeToken statusCodeToken;
            if (int.TryParse(httpStatusCode.Value, out var code) && code >= 400 && code < 600)
            {
                statusCodeToken = new StatusCodeToken.Failure(
                    new HeadersReader<IErrorResponseReader>(
                        this.httpResponseMessage, 
                        this.httpResponseMessage.Headers.GetEnumerator(),
                        (bytes, index) => new ErrorResponseReader()));
            }
            else
            {
                statusCodeToken = new StatusCodeToken.Success(
                    new HeadersReader<IBodyReader>(
                        this.httpResponseMessage,
                        this.httpResponseMessage.Headers.GetEnumerator(),
                        (bytes, index) => new BodyReader(bytes, index)));
            }

            return await Task.FromResult((statusCodeToken,  httpStatusCode)).ConfigureAwait(false);
        }
    }

    internal sealed class HeadersReader<T> : IHeadersReader<T>
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;
        private readonly Func<byte[], long, T> bodyReaderFactory;

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpResponseMessage"/> or <paramref name="headers"/> or <paramref name="bodyReaderFactory"/> is <see langword="null"/></exception>
        internal HeadersReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers,
            Func<byte[], long, T> bodyReaderFactory)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
            this.bodyReaderFactory = bodyReaderFactory;
        }

        public async Task<HeadersToken<T>> Read()
        {
            if (!this.headers.MoveNext())
            {
                var bytes = await this.httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                var index = 0L;
                var bodyReader = this.bodyReaderFactory(bytes, index);
                return new HeadersToken<T>.Body(bodyReader);
            }
            else
            {
                return new HeadersToken<T>.Header(new HeaderReader<T>(this.httpResponseMessage, this.headers, this.bodyReaderFactory));
            }
        }
    }

    internal sealed class HeaderReader<T> : IHeaderReader<T>
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;
        private readonly Func<byte[], long, T> bodyReaderFactory;


        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpResponseMessage"/> or <paramref name="headers"/> or <paramref name="bodyReaderFactory"/> is <see langword="null"/></exception>
        internal HeaderReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers,
            Func<byte[], long, T> bodyReaderFactory)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
            this.bodyReaderFactory = bodyReaderFactory;
        }

        public async Task<IHeaderKvpReader<T>> Read()
        {
            return await Task.FromResult(new HeaderKvpReader<T>(this.httpResponseMessage, this.headers, this.bodyReaderFactory)).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderKvpReader<T> : IHeaderKvpReader<T>
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;
        private readonly Func<byte[], long, T> bodyReaderFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <param name="headers"></param>
        /// <param name="bodyReaderFactory"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpResponseMessage"/> or <paramref name="headers"/> or <paramref name="bodyReaderFactory"/> is <see langword="null"/></exception>
        internal HeaderKvpReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers,
            Func<byte[], long, T> bodyReaderFactory)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
            this.bodyReaderFactory = bodyReaderFactory;
        }

        public async Task<IHeaderKeyReader<T>> Read()
        {
            return await Task.FromResult(new HeaderKeyReader<T>(this.httpResponseMessage, this.headers, this.bodyReaderFactory)).ConfigureAwait(false);
        }
    }

    internal sealed class HeaderKeyReader<T> : IHeaderKeyReader<T>
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;
        private readonly Func<byte[], long, T> bodyReaderFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <param name="headers"></param>
        /// <param name="bodyReaderFactory"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpResponseMessage"/> or <paramref name="headers"/> or <paramref name="bodyReaderFactory"/> is <see langword="null"/></exception>
        internal HeaderKeyReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers,
            Func<byte[], long, T> bodyReaderFactory)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
            this.bodyReaderFactory = bodyReaderFactory;
        }

        public async Task<(HeaderKeyToken<T> HeaderKeyToken, HeaderKey HeaderKey)> Read()
        {
            var header = this.headers.Current;
            var headerKey = new HeaderKey(header.Key);

            var values = header.Value.GetEnumerator();
            if (!values.MoveNext())
            {
                var token = new HeaderKeyToken<T>.Headers(new HeadersReader<T>(this.httpResponseMessage, this.headers, this.bodyReaderFactory));

                return await Task.FromResult((token, headerKey)).ConfigureAwait(false);
            }
            else
            {
                var token = new HeaderKeyToken<T>.HeaderValue(new HeaderValueReader<T>(this.httpResponseMessage, this.headers, values, this.bodyReaderFactory));

                return await Task.FromResult((token, headerKey)).ConfigureAwait(false);
            }
        }
    }

    internal sealed class HeaderValueReader<T> : IHeaderValueReader<T>
    {
        private readonly HttpResponseMessage httpResponseMessage;
        private readonly IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers;
        private readonly IEnumerator<string> values;
        private readonly Func<byte[], long, T> bodyReaderFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpResponseMessage"></param>
        /// <param name="headers"></param>
        /// <param name="values"></param>
        /// <param name="bodyReaderFactory"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpResponseMessage"/> or <paramref name="headers"/> <paramref name="values"/> or <paramref name="bodyReaderFactory"/> is <see langword="null"/></exception>
        internal HeaderValueReader(
            HttpResponseMessage httpResponseMessage,
            IEnumerator<KeyValuePair<string, IEnumerable<string>>> headers,
            IEnumerator<string> values,
            Func<byte[], long, T> bodyReaderFactory)
        {
            this.httpResponseMessage = httpResponseMessage;
            this.headers = headers;
            this.values = values;
            this.bodyReaderFactory = bodyReaderFactory;
        }

        public async Task<(HeaderValueToken<T> HeaderValueToken, HeaderValue HeaderValue)> Read()
        {
            var headerValue = new HeaderValue(this.values.Current);
            if (!this.values.MoveNext())
            {
                var token = new HeaderValueToken<T>.Headers(new HeadersReader<T>(this.httpResponseMessage, this.headers, this.bodyReaderFactory));

                return await Task.FromResult((token, headerValue)).ConfigureAwait(false);
            }
            else
            {
                var token = new HeaderValueToken<T>.HeaderValue(new HeaderValueReader<T>(this.httpResponseMessage, this.headers, this.values, this.bodyReaderFactory));

                return await Task.FromResult((token, headerValue)).ConfigureAwait(false);
            }
        }
    }

    internal sealed class BodyReader : IBodyReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            if (jsonReader.TokenType != JsonTokenType.PropertyName)
            {
                throw new ReadException("TODO");
            }

            var receivedPropertyName = jsonReader.GetString();
            if (string.IsNullOrEmpty(receivedPropertyName))
            {
                throw new ReadException("TODO");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
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
                    throw new ReadException("TODO");
            }
        }
    }

    internal sealed class LiteralReader : ILiteralReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
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
                    throw new ReadException("TODO");
            }
        }
    }

    internal sealed class FalseReader : IFalseReader
    {
        private readonly byte[] bytes;
        private readonly long index;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            if (jsonReader.TokenType != JsonTokenType.False)
            {
                throw new ReadException("TODO");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            if (jsonReader.TokenType != JsonTokenType.Number)
            {
                throw new ReadException("TODO");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            if (jsonReader.TokenType != JsonTokenType.True)
            {
                throw new ReadException("TODO");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            if (jsonReader.TokenType != JsonTokenType.Null)
            {
                throw new ReadException("TODO");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bytes"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is TODO</exception>
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
                throw new ReadException("TODO invalid JSON"); //// TODO do you want a dedicated exception type for the underlying format being broken? so, something that differentiates between "bad odata syntax (like two properties with the same name)" and "invalid JSON/XML/whatever"?
            }

            if (jsonReader.TokenType != JsonTokenType.String)
            {
                throw new ReadException("TODO");
            }

            var receivedPropertyName = jsonReader.GetString();
            if (receivedPropertyName == null)
            {
                throw new ReadException("TODO what would a null value even mean here? is this just a jsonreader deficiency?");
            }

            var stringToken = new StringToken(receivedPropertyName);
            var bodyReader = new BodyReader(this.bytes, jsonReader.BytesConsumed);

            return await Task.FromResult((bodyReader, stringToken)).ConfigureAwait(false);
        }
    }

    internal sealed class ErrorResponseReader : IErrorResponseReader
    {
    }
}
