namespace OddTrotter.CalendarV1.Readers
{
    using System;

    public interface IObjectValueReader<out TNextReader> : IReader<IObjectValueToken<TNextReader>>
    {
    }

    public interface IObjectValueToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IObjectRootControlInformationReader<IObjectValueReader<TNextReader>>, TResult> objectRootControlInformationReader,
            Func<IObjectRootAnnotationReader<IObjectValueReader<TNextReader>>, TResult> objectRootAnnotationReader,
            Func<IPropertyReader<IObjectValueReader<TNextReader>>, TResult> propertyReader,
            Func<TNextReader, TResult> nextReader);
    }

    public interface IObjectRootControlInformationReader<out TNextReader> : IReader<IResponseRootControlInformationToken<TNextReader>>
    {
    }

    public interface IObjectRootControlInformationToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IObjectRootEtagReader<TNextReader>, TResult> etagReader,
            Func<IObjectRootUnknownControlInformationReader<TNextReader>, TResult> unknownControlInformationReader);
    }

    public interface IObjectRootEtagReader<out TNextReader> : IReader<TNextReader, Etag>
    {
    }

    public sealed class Etag
    {
        private Etag()
        {
        }
    }

    public interface IObjectRootUnknownControlInformationReader<out TNextReader> : IReader<IObjectRootUnknownControlInformationNameReader<TNextReader>>
    {
        // from [the standard](https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_ControlInformation):
        // > Receivers that encounter unknown annotations in any namespace or unknown control information MUST NOT stop processing and MUST NOT signal an error.
        //
        // we need to be able to handle things that look like control information, but are unknown to us at the time of implementation
    }

    public interface IObjectRootUnknownControlInformationNameReader<out TNextReader> : IReader<IObjectRootUnknownControlInformationValueReader<TNextReader>, ControlInformationName>
    {
    }

    public interface IObjectRootUnknownControlInformationValueReader<out TNextReader> : IReader<TNextReader, ControlInformationValue>
    {
    }

    public interface IObjectRootAnnotationReader<out TNextReader> : IReader<IObjectRootAnnotationNameReader<TNextReader>>
    {
    }

    public interface IObjectRootAnnotationNameReader<out TNextReader> : IReader<IObjectRootAnnotationValueReader<TNextReader>, AnnotationName>
    {
    }

    public interface IObjectRootAnnotationValueReader<out TNextReader> : IReader<TNextReader, AnnotationValue>
    {
    }
}
