namespace OddTrotter.CalendarV1.Readers
{
    using System;

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

    public interface IRootUnknownControlInformationReader<out TNextReader> : IReader<IRootUnknownControlInformationNameReader<TNextReader>>
    {
        // from [the standard](https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_ControlInformation):
        // > Receivers that encounter unknown annotations in any namespace or unknown control information MUST NOT stop processing and MUST NOT signal an error.
        //
        // we need to be able to handle things that look like control information, but are unknown to us at the time of implementation
    }

    public interface IRootUnknownControlInformationNameReader<out TNextReader> : IReader<IRootUnknownControlInformationValueReader<TNextReader>, ControlInformationName>
    {
    }

    public sealed class ControlInformationName
    {
        private ControlInformationName()
        {
        }
    }

    public interface IRootUnknownControlInformationValueReader<out TNextReader> : IReader<TNextReader, ControlInformationValue>
    {
    }

    public sealed class ControlInformationValue
    {
        private ControlInformationValue()
        {
            //// TODO this is a DU between strings and each of the different primitives //// TODO actually, could it be any json token?
        }
    }
}
