namespace OddTrotter.CalendarV1.Tokenization.Readers
{
    using System;
    using System.ComponentModel.Design;

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
            Func<IResponseRootControlInformationReader<IGetResponseBodyAfterOdataContextReader>, TResult> responseRootControlInformationReader,
            Func<IResponseRootAnnotationReader<IGetResponseBodyAfterOdataContextReader>, TResult> responseRootAnnotationReader,
            Func<IPropertyReader<IGetResponseBodyAfterOdataContextReader>, TResult> propertyReader,
            Func<Nothing, TResult> terminal);
    }
















    public interface IPropertyReader<out TNextReader> : IReader<IPropertyNameReader<TNextReader>>
    {
    }

    public interface IPropertyNameReader<out TNextReader> : IReader<IPropertyNameToken<TNextReader>, PropertyName>
    {
    }

    public sealed class PropertyName
    {
        internal PropertyName(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    public interface IPropertyValueReader
    {
    }

    public interface IPropertyNameToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IPropertyControlInformationReader<TNextReader>, TResult> propertyControlInformationReader,
            Func<IPropertyAnnotationReader<TNextReader>, TResult> propertyAnnotationReader,
            Func<IPropertyValueReader<TNextReader>, TResult> propertyValueReader);
    }

    public interface IPropertyControlInformationReader<out TNextReader> : IReader<IPropertyControlInformationToken<TNextReader>>
    {
    }

    public interface IPropertyControlInformationToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IPropertyAssociationLinkReader<TNextReader>, TResult> propertyAssociationLinkReader,
            Func<IPropertyNavigationLinkReader<TNextReader>, TResult> propertyNavigationLinkReader,
            Func<IPropertyUnknownControlInformationReader<TNextReader>, TResult> propertyUnknownControlInformationReader);
    }


    public interface IPropertyAssociationLinkReader<out TNextReader> : IReader<TNextReader, AssociationLink>
    {
    }

    public sealed class AssociationLink
    {
        private AssociationLink()
        {
        }
    }

    public interface IPropertyNavigationLinkReader<out TNextReader> : IReader<TNextReader, NavigationLink>
    {
    }

    public sealed class NavigationLink
    {
        private NavigationLink()
        {
        }
    }

    public interface IPropertyUnknownControlInformationReader<out TNextReader> : IReader<IPropertyUnknownControlInformationNameReader<TNextReader>>
    {
        // from [the standard](https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_ControlInformation):
        // > Receivers that encounter unknown annotations in any namespace or unknown control information MUST NOT stop processing and MUST NOT signal an error.
        //
        // we need to be able to handle things that look like control information, but are unknown to us at the time of implementation
    }

    public interface IPropertyUnknownControlInformationNameReader<out TNextReader> : IReader<IPropertyUnknownControlInformationValueReader<TNextReader>, ControlInformationName>
    {
    }

    public interface IPropertyUnknownControlInformationValueReader<out TNextReader> : IReader<TNextReader, ControlInformationValue>
    {
    }

    public interface IPropertyAnnotationReader<out TNextReader> : IReader<IPropertyAnnotationNameReader<TNextReader>>
    {
    }

    public interface IPropertyAnnotationNameReader<out TNextReader> : IReader<IPropertyAnnotationValueReader<TNextReader>, AnnotationName>
    {
    }

    public interface IPropertyAnnotationValueReader<out TNextReader> : IReader<TNextReader, AnnotationValue>
    {
    }

    public interface IPropertyValueReader<out TNextReader> : IReader<IPropertyNameToken<TNextReader>>
    {
    }

    public interface IPropertyValueToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<INullValueReader<TNextReader>, TResult> nullValueReader,
            Func<IPrimitiveValueReader<TNextReader>, TResult> primitiveValueReader,
            Func<IStringValueReader<TNextReader>, TResult> stringValueReader,
            Func<IObjectValueReader<TNextReader>, TResult> objectValueReader,
            Func<ICollectionValueReader<TNextReader>, TResult> collectionValueReader);

        //// TODO should there be a "datetimevaluereader" or something? basically, odata overloads strings sometimes, what are all of the times it does that?
    }

    public interface INullValueReader<out TNextReader> : IReader<TNextReader>
    {
    }

    public interface IPrimitiveValueReader<out TNextReader> : IReader<IPrimitiveValueToken<TNextReader>>
    {

    }

    public interface IPrimitiveValueToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<IIntValueReader<TNextReader>, TResult> intValueReader,
            Func<IBoolValueReader<TNextReader>, TResult> boolValueReader);
    }

    public interface IIntValueReader<out TNextReader> : IReader<TNextReader, IntValue>
    {
    }

    public sealed class IntValue
    {
        private IntValue()
        {
        }
    }

    public interface IBoolValueReader<out TNextReader> : IReader<TNextReader, BoolValue>
    {
    }

    public sealed class BoolValue
    {
        private BoolValue()
        {
        }
    }

    public interface IStringValueReader<out TNextReader> : IReader<TNextReader, StringValue>
    {
    }
    `
    public sealed class StringValue
    {
        private StringValue()
        {
        }
    }

    public interface ICollectionValueReader<out TNextReader> : IReader<ICollectionValueToken<TNextReader>>
    {
    }

    public interface ICollectionValueToken<out TNextReader>
    {
        TResult Apply<TResult>(
            // it's really weird to have a collection that might be heterogenous such that it has objects, strings, and primitives, but i think it's technically legal for something like `Collection(Edm.Untyped)`, so we need to support it and the caller will have to check the validity against the EDM model
            Func<INullValueReader<ICollectionValueReader<TNextReader>>, TResult> nullValueReader,
            Func<IPrimitiveValueReader<ICollectionValueReader<TNextReader>>, TResult> primitiveValueReader,
            Func<IStringValueReader<ICollectionValueReader<TNextReader>>, TResult> stringValueReader,
            Func<IObjectValueReader<ICollectionValueReader<TNextReader>>, TResult> objectValueReader,
            //// TODO nested collections?
            Func<TNextReader, TResult> nextReader);

        //// TODO should there be a "datetimevaluereader" or something? basically, odata overloads strings sometimes, what are all of the times it does that?
    }
}
