namespace OddTrotter.CalendarV1.Readers
{
    using System;
    using System.IO;

    public interface IGetResponseReader : IReader<IGetResponseHeadersReader>
    {
    }

    public interface IGetResponseHeadersReader : IReader<IGetResponseHeadersToken>
    {
    }

    public interface IGetResponseHeadersToken
    {
        TResult Apply<TResult>(
            Func<IGetResponseHeaderReader, TResult> getResponseHeaderReader,
            Func<IGetResponseBodyReader, TResult> getResponseBodyReader);
    }

    public interface IGetResponseHeaderReader : IReader<IGetResponseHeaderToken>
    {
    }

    public interface IGetResponseHeaderToken
    {
        TResult Apply<TResult>(
            Func<ICustomHeaderReader<IGetResponseHeadersReader>, TResult> customHeaderReader);
    }

    public interface IGetResponseBodyReader : IReader<IOdataContextReader<IGetResponseBodyAfterOdataContextReader>>
    {
        // according to [the standard](https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_ControlInformationcontextodatacontex):
        // > The context control information is not returned if metadata=none is requested. Otherwise it MUST be the first property of any JSON response.
        // 
        // the odata.context is the first control information, if present
    }

    public interface IGetResponseBodyAfterOdataContextReader : IReader<IGetResponseBodyAfterOdataContextToken>
    {
    }

    public interface IGetResponseBodyAfterOdataContextToken
    {
        TResult Apply<TResult>(
            Func<IRootControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> controlInformationReader,,
            Func<IRootAnnotationReader<IGetResponseBodyAfterOdataContextReader>, TResult> rootAnnotationReader,
            Func<IPropertyControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyControlInformationReader,
            Func<IPropertyAnnotationReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyAnnotationReader,
            Func<IPropertyReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyReader,
            Func<System.Nothing, TResult> terminal);
    }

    public interface IRootControlInformationReader<out TNextReader> : IReader<IRootControlInformationToken<TNextReader>>
    {
    }

    public interface IRootControlInformationToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IRootNextLinkReader<TNextReader>, TResult> nextLinkReader,
            Func<IRootUnknownControlInformationReader<TNextReader>, TResult> unknownControlInformationReader);
    }

    public interface IRootNextLinkReader<out TNextReader> : IReader<TNextReader, NextLink>
    {
    }

    public sealed class NextLink
    {
        private NextLink()
        {
        }
    }

    public interface IRootUnknownControlInformationReader<out TNextReader> //// TODO document where in the standard this is
    {
    }










    public interface IRootAnnotationReader<out TNextReader>
    {
    }

    public interface IPropertyControlInformationReader<out TNextReader>
    {
    }

    public interface IPropertyAnnotationReader<out TNextReader>
    {
    }

    public interface IPropertyReader<out TNextReader>
    {
    }
}
