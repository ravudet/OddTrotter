namespace OddTrotter.CalendarV1.Readers
{
    using System;

    public interface IRequestRootControlInformationReader<out TNextReader> : IReader<IRequestRootControlInformationToken<TNextReader>>
    {
    }

    public interface IRequestRootControlInformationToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IRequestRootNextLinkReader<TNextReader>, TResult> nextLinkReader,
            Func<IRequestRootUnknownControlInformationReader<TNextReader>, TResult> unknownControlInformationReader);
    }

    public interface IRequestRootNextLinkReader<out TNextReader> : IReader<TNextReader, NextLink>
    {
    }

    public sealed class NextLink
    {
        private NextLink()
        {
        }
    }

    public interface IRequestRootUnknownControlInformationReader<out TNextReader> : IReader<IRequestRootUnknownControlInformationNameReader<TNextReader>>
    {
        // from [the standard](https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_ControlInformation):
        // > Receivers that encounter unknown annotations in any namespace or unknown control information MUST NOT stop processing and MUST NOT signal an error.
        //
        // we need to be able to handle things that look like control information, but are unknown to us at the time of implementation
    }

    public interface IRequestRootUnknownControlInformationNameReader<out TNextReader> : IReader<IRequestRootUnknownControlInformationValueReader<TNextReader>, ControlInformationName>
    {
    }

    public sealed class ControlInformationName
    {
        private ControlInformationName()
        {
        }
    }

    public interface IRequestRootUnknownControlInformationValueReader<out TNextReader> : IReader<TNextReader, ControlInformationValue>
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
