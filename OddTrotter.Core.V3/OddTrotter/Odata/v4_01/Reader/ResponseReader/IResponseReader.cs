namespace OddTrotter.Odata.v4_01.Reader.ResponseReader
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader;





    //// TODO look at `playgroundtests.ReadingFromDeadNetworkStream` to make sure that the ioexceptions you surface aren't accidentally httprequestexceptions and to make sure that payload content is completely streamed; do the same for the writer


    internal interface IResponseReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IStatusCodeReader> Read();
    }

    internal interface IStatusCodeReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(StatusCodeToken StatusCodeToken, HttpStatusCode HttpStatusCode)> Read();
    }

    internal abstract class StatusCodeToken
    {
        private StatusCodeToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="successMap"></param>
        /// <param name="failureMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="successMap"/> or <paramref name="failureMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<Success, TResult> successMap,
            Func<Failure, TResult> failureMap)
        {
            if (this is Success success)
            {
                return successMap(success);
            }
            else if (this is Failure failure)
            {
                return failureMap(failure);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class Success : StatusCodeToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            public Success(IHeadersReader<IBodyReader> reader)
            {
                Reader = reader;
            }

            public IHeadersReader<IBodyReader> Reader { get; }
        }

        internal sealed class Failure : StatusCodeToken
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            public Failure(IHeadersReader<IErrorResponseReader> reader)
            {
                Reader = reader;
            }

            public IHeadersReader<IErrorResponseReader> Reader { get; }
        }
    }

    internal interface IHeadersReader<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<HeadersToken<T>> Read();
    }

    internal abstract class HeadersToken<T>
    {
        private HeadersToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="headerMap"></param>
        /// <param name="bodyMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerMap"/> or <paramref name="bodyMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<Header, TResult> headerMap,
            Func<Body, TResult> bodyMap)
        {
            if (this is Header header)
            {
                return headerMap(header);
            }
            else if (this is Body body)
            {
                return bodyMap(body);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        internal sealed class Header : HeadersToken<T>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Header(IHeaderReader<T> reader)
            {
                Reader = reader;
            }

            public IHeaderReader<T> Reader { get; }
        }

        internal sealed class Body : HeadersToken<T>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
            internal Body(T reader)
            {
                Reader = reader;
            }

            public T Reader { get; }
        }
    }

    internal interface IHeaderReader<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IHeaderKvpReader<T>> Read();
    }

    internal interface IHeaderKvpReader<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IHeaderKeyReader<T>> Read();
    }

    internal interface IHeaderKeyReader<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(HeaderKeyToken<T> HeaderKeyToken, HeaderKey HeaderKey)> Read();
    }

    internal abstract class HeaderKeyToken<T>
    {
        private HeaderKeyToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="headerValueMap"></param>
        /// <param name="headersMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerValueMap"/> or <paramref name="headersMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<HeaderValue, TResult> headerValueMap,
            Func<Headers, TResult> headersMap)
        {
            if (this is HeaderValue headerValue)
            {
                return headerValueMap(headerValue);
            }
            else if (this is Headers headers)
            {
                return headersMap(headers);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class HeaderValue : HeaderKeyToken<T>
        {
            internal HeaderValue(IHeaderValueReader<T> reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader<T> Reader { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class Headers : HeaderKeyToken<T>
        {
            internal Headers(IHeadersReader<T> reader)
            {
                Reader = reader;
            }

            public IHeadersReader<T> Reader { get; }
        }
    }

    internal interface IHeaderValueReader<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(HeaderValueToken<T> HeaderValueToken, HeaderValue HeaderValue)> Read();
    }

    internal abstract class HeaderValueToken<T>
    {
        private HeaderValueToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="headerValueMap"></param>
        /// <param name="headersMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headerValueMap"/> or <paramref name="headersMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<HeaderValue, TResult> headerValueMap,
            Func<Headers, TResult> headersMap)
        {
            if (this is HeaderValue headerValue)
            {
                return headerValueMap(headerValue);
            }
            else if (this is Headers headers)
            {
                return headersMap(headers);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class HeaderValue : HeaderValueToken<T>
        {
            internal HeaderValue(IHeaderValueReader<T> reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader<T> Reader { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class Headers : HeaderValueToken<T>
        {
            internal Headers(IHeadersReader<T> reader)
            {
                Reader = reader;
            }

            public IHeadersReader<T> Reader { get; }
        }
    }

    internal interface IBodyReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<BodyToken> Read();
    }

    internal abstract class BodyToken
    {
        private BodyToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyMap"></param>
        /// <param name="endMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyMap"/> or <paramref name="endMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<Property, TResult> propertyMap,
            Func<End, TResult> endMap)
        {
            if (this is Property property)
            {
                return propertyMap(property);
            }
            else if (this is End end)
            {
                return endMap(end);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class Property : BodyToken
        {
            public Property(IPropertyReader reader)
            {
                Reader = reader;
            }

            public IPropertyReader Reader { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class End : BodyToken
        {
            private End()
            {
            }

            public static End Instance { get; } = new End();
        }
    }

    internal interface IPropertyReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IPropertyNameReader> Read();
    }

    internal interface IPropertyNameReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IPropertyValueReader PropertyValueReader, PropertyName PropertyName)> Read();
    }

    internal interface IPropertyValueReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<PropertyValueToken> Read();
    }

    internal abstract class PropertyValueToken
    {
        private PropertyValueToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="literalMap"></param>
        /// <param name="nullMap"></param>
        /// <param name="stringMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="literalMap"/> or <paramref name="nullMap"/> or <paramref name="stringMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<Literal, TResult> literalMap,
            Func<Null, TResult> nullMap,
            Func<String, TResult> stringMap)
        {
            if (this is Literal literal)
            {
                return literalMap(literal);
            }
            else if (this is Null @null)
            {
                return nullMap(@null);
            }
            else if (this is String @string)
            {
                return stringMap(@string);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class Literal : PropertyValueToken
        {
            internal Literal(ILiteralReader reader)
            {
                Reader = reader;
            }

            public ILiteralReader Reader { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class Null : PropertyValueToken
        {
            internal Null(INullReader reader)
            {
                Reader = reader;
            }

            public INullReader Reader { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class String : PropertyValueToken
        {
            internal String(IStringReader reader)
            {
                Reader = reader;
            }

            public IStringReader Reader { get; }
        }

        //// TODO add objects and collections
    }

    internal interface ILiteralReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<LiteralToken> Read();
    }

    internal abstract class LiteralToken
    {
        private LiteralToken()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="trueMap"></param>
        /// <param name="falseMap"></param>
        /// <param name="numberMap"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="trueMap"/> or <paramref name="falseMap"/> or <paramref name="numberMap"/> is <see langword="null"/></exception>
        internal TResult Apply<TResult>(
            Func<True, TResult> trueMap,
            Func<False, TResult> falseMap,
            Func<Number, TResult> numberMap)
        {
            if (this is True @true)
            {
                return trueMap(@true);
            }
            else if (this is False @false)
            {
                return falseMap(@false);
            }
            else if (this is Number number)
            {
                return numberMap(number);
            }
            else
            {
                throw new Exception("TODO visitor");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class True : LiteralToken
        {
            internal True(ITrueReader reader)
            {
                Reader = reader;
            }

            public ITrueReader Reader { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class False : LiteralToken
        {
            internal False(IFalseReader reader)
            {
                Reader = reader;
            }

            public IFalseReader Reader { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is <see langword="null"/></exception>
        internal sealed class Number : LiteralToken
        {
            internal Number(INumberReader reader)
            {
                Reader = reader;
            }

            public INumberReader Reader { get; }
        }
    }

    internal interface ITrueReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IBodyReader BodyReader, TrueToken TrueToken)> Read();
    }

    internal interface IFalseReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IBodyReader BodyReader, FalseToken FalseToken)> Read();
    }

    internal interface INumberReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IBodyReader BodyReader, Number Number)> Read();
    }

    internal interface INullReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IBodyReader BodyReader, NullToken NullToken)> Read();
    }

    internal interface IStringReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(IBodyReader BodyReader, StringToken StringToken)> Read();
    }

    internal interface IErrorResponseReader
    {
    }
}
