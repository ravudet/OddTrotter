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
        Task<(IHeadersReader HeadersReader, HttpStatusCode HttpStatusCode)> Read();
    }

    internal interface IHeadersReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<HeadersToken> Read();
    }

    internal abstract class HeadersToken
    {
        private HeadersToken()
        {
        }

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

        internal sealed class Header : HeadersToken
        {
            internal Header(IHeaderReader reader)
            {
                Reader = reader;
            }

            public IHeaderReader Reader { get; }
        }

        internal sealed class Body : HeadersToken
        {
            internal Body(IBodyReader reader)
            {
                Reader = reader;
            }

            public IBodyReader Reader { get; }
        }
    }

    internal interface IHeaderReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IHeaderKvpReader> Read();
    }

    internal interface IHeaderKvpReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<IHeaderKeyReader> Read();
    }

    internal interface IHeaderKeyReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(HeaderKeyToken HeaderKeyToken, HeaderKey HeaderKey)> Read();
    }

    internal abstract class HeaderKeyToken
    {
        private HeaderKeyToken()
        {
        }

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

        internal sealed class HeaderValue : HeaderKeyToken
        {
            internal HeaderValue(IHeaderValueReader reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader Reader { get; }
        }

        internal sealed class Headers : HeaderKeyToken
        {
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
        }
    }

    internal interface IHeaderValueReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException" awaited="true">thrown if an error occurred reading from the underlying payload</exception>
        /// <exception cref="HttpRequestException" awaited="true">thrown if an error occurred while receiving the payload from the service</exception>
        /// <exception cref="ReadException" awaited="true">thrown if the payload is not valid odata</exception>
        Task<(HeaderValueToken HeaderValueToken, HeaderValue HeaderValue)> Read();
    }

    internal abstract class HeaderValueToken
    {
        private HeaderValueToken()
        {
        }

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

        internal sealed class HeaderValue : HeaderValueToken
        {
            internal HeaderValue(IHeaderValueReader reader)
            {
                Reader = reader;
            }

            public IHeaderValueReader Reader { get; }
        }

        internal sealed class Headers : HeaderValueToken
        {
            internal Headers(IHeadersReader reader)
            {
                Reader = reader;
            }

            public IHeadersReader Reader { get; }
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

        internal sealed class Property : BodyToken
        {
            public Property(IPropertyReader reader)
            {
                Reader = reader;
            }

            public IPropertyReader Reader { get; }
        }

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

        internal sealed class Literal : PropertyValueToken
        {
            internal Literal(ILiteralReader reader)
            {
                Reader = reader;
            }

            public ILiteralReader Reader { get; }
        }

        internal sealed class Null : PropertyValueToken
        {
            internal Null(INullReader reader)
            {
                Reader = reader;
            }

            public INullReader Reader { get; }
        }

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

        internal sealed class True : LiteralToken
        {
            internal True(ITrueReader reader)
            {
                Reader = reader;
            }

            public ITrueReader Reader { get; }
        }

        internal sealed class False : LiteralToken
        {
            internal False(IFalseReader reader)
            {
                Reader = reader;
            }

            public IFalseReader Reader { get; }
        }

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
}
