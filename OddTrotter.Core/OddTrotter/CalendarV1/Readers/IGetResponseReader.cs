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
    }

    public interface IGetResponseBodyAfterOdataContextReader : IReader<IGetResponseBodyAfterOdataContextToken>
    {
    }

    public interface IGetResponseBodyAfterOdataContextToken
    {
        TResult Apply<TResult>(
            Func<IControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> controlInformationReader,
            Func<IPropertyReader<IGetResponseBodyAfterOdataContextToken>, TResult> propertyReader,
            Func<System.Nothing, TResult> terminal);
    }

    public interface IControlInformationReader<out TNextReader>
    {
    }

    public interface IPropertyReader<out TNextReader>
    {
    }
}
