namespace OddTrotter.Odata.v4_01.Reader.ResponseReader
{
    using System;
    using System.Threading.Tasks;

    using OddTrotter.Odata.v4_01.Reader;



    //// TODO you need to implement a requestreader, a requestwriter, a responsereader, and a responsewriter that ensure that payload content is completely streamed


    internal interface IResponseReader
    {
        Task<IStatusCodeReader> Read();
    }

    internal interface IStatusCodeReader
    {
        Task<IHeadersReader> Read(out HttpStatusCode httpStatusCode);
    }

    internal interface IHeadersReader
    {
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
        Task<IHeaderKvpReader> Read();
    }

    internal interface IHeaderKvpReader
    {
        Task<IHeaderKeyReader> Read();
    }

    internal interface IHeaderKeyReader
    {
        Task<HeaderKeyToken> Read(out HeaderKey headerKey);
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
        Task<HeaderValueToken> Read(out HeaderValue headerValue);
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
        Task<IPropertyNameReader> Read();
    }

    internal interface IPropertyNameReader
    {
        Task<IPropertyValueReader> Read(out PropertyName propertyName);
    }

    internal interface IPropertyValueReader
    {
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
        Task<IBodyReader> Read(out TrueToken trueToken);
    }

    internal interface IFalseReader
    {
        Task<IBodyReader> Read(out FalseToken falseToken);
    }

    internal interface INumberReader
    {
        Task<IBodyReader> Read(out Number number);
    }

    internal interface INullReader
    {
        Task<IBodyReader> Read(out NullToken nullToken);
    }

    internal interface IStringReader
    {
        Task<IBodyReader> Read(out StringToken stringToken);
    }
}
