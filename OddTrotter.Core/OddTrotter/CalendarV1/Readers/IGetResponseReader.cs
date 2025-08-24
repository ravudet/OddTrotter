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

    public interface IGetResponseBodyReader : IReader<IOdataContextReader<IGetResponseBodyAfterOdataContextReader>> //// TODO document where in the standard this is
    {
    }

    public interface IGetResponseBodyAfterOdataContextReader : IReader<IGetResponseBodyAfterOdataContextToken>
    {
    }

    public interface IGetResponseBodyAfterOdataContextToken
    {
        TResult Apply<TResult>(
            Func<IRootControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> controlInformationReader,
            Func<IPropertyReader<IGetResponseBodyAfterOdataContextToken>, TResult> propertyReader,
            //// TODO you are here
            //// TODO add the other readers here
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
