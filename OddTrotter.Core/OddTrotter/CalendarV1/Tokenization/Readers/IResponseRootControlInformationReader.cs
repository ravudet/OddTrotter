namespace OddTrotter.CalendarV1.Tokenization.Readers
{
    using System;

    public interface IResponseRootControlInformationReader<out TNextReader> : IReader<IResponseRootControlInformationToken<TNextReader>>
    {
    }

    public interface IResponseRootControlInformationToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IResponseRootNextLinkReader<TNextReader>, TResult> nextLinkReader,
            Func<IResponseRootUnknownControlInformationReader<TNextReader>, TResult> unknownControlInformationReader);
    }

    public interface IResponseRootNextLinkReader<out TNextReader> : IReader<TNextReader, NextLink>
    {
    }

    public sealed class NextLink
    {
        internal NextLink(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    public interface IResponseRootUnknownControlInformationReader<out TNextReader> : IReader<IResponseRootUnknownControlInformationNameReader<TNextReader>>
    {
        // from [the standard](https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_ControlInformation):
        // > Receivers that encounter unknown annotations in any namespace or unknown control information MUST NOT stop processing and MUST NOT signal an error.
        //
        // we need to be able to handle things that look like control information, but are unknown to us at the time of implementation
    }

    public interface IResponseRootUnknownControlInformationNameReader<out TNextReader> : IReader<IResponseRootUnknownControlInformationValueReader<TNextReader>, ControlInformationName>
    {
    }

    public sealed class ControlInformationName
    {
        internal ControlInformationName(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    public interface IResponseRootUnknownControlInformationValueReader<out TNextReader> : IReader<TNextReader, ControlInformationValue>
    {
    }

    public abstract class ControlInformationValue
    {
        private ControlInformationValue()
        {
            //// TODO this is a DU between strings and each of the different primitives //// TODO actually, could it be any json token?
        }
    }
}
