namespace OddTrotter.CalendarV1.Readers
{
    using System;

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
        // from [the standard](https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_ControlInformationcontextodatacontex):
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
            Func<IRootControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> controlInformationReader,
            Func<IRootAnnotationReader<IGetResponseBodyAfterOdataContextReader>, TResult> rootAnnotationReader,
            Func<IPropertyControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyControlInformationReader,
            Func<IPropertyAnnotationReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyAnnotationReader,
            Func<IPropertyReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyReader,
            Func<System.Nothing, TResult> terminal);
    }

    public interface IRootAnnotationReader<out TNextReader> : IReader<IRootAnnotationNameReader<TNextReader>>
    {
    }

    public interface IRootAnnotationNameReader<out TNextReader> : IReader<IRootAnnotationValueReader<TNextReader>, AnnotationName>
    {
    }

    public sealed class AnnotationName
    {
        private AnnotationName()
        {
        }
    }

    public interface IRootAnnotationValueReader<out TNextReader>
    {
    }

    public sealed class AnnotationValue
    {
        private AnnotationValue()
        {
            //// TODO this might be a DU between strings and each of the different primitives; see if you can find the answer in the standard //// TODO actually, it seems to be *any* JSON token, including an object
        }
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
