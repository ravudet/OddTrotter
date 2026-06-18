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
        internal AssociationLink(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    public interface IPropertyNavigationLinkReader<out TNextReader> : IReader<TNextReader, NavigationLink>
    {
    }

    public sealed class NavigationLink
    {
        internal NavigationLink(string value)
        {
            Value = value;
        }

        internal string Value { get; }
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



/*
json = ws value ws

value = false / null / true / object / array / number / string

false = %x66.61.6C.73.65  ; "false"
null  = %x6E.75.6C.6C     ; "null"
true  = %x74.72.75.65     ; "true"

// object = %x7B ws [ member *( %x2C ws member ) ] ws %x7D
// member = string ws %x3A ws value

object = %x7B ws *( member %x2C ws ) %x7D
member = string ws %x3A ws value

array = %x5B ws [ value *( %x2C ws value ) ] ws %x5D

number = [ "-" ] int [ frac ] [ exp ]
int    = "0" / ( digit1-9 *digit )
frac   = "." 1*digit
exp    = ( "e" / "E" ) [ "+" / "-" ] 1*digit

string = %x22 *char %x22
char   = unescaped / escape ( %x22 / %x5C / %x2F / %x62 / %x66 / %x6E / %x72 / %x74 / unicode )
escape = %x5C
unicode = %x75 4HEXDIG
unescaped = %x20-21 / %x23-5B / %x5D-10FFFF

ws = *(%x20 / %x09 / %x0A / %x0D)

digit = %x30-39
digit1-9 = %x31-39
HEXDIG = digit / %x41-46 / %x61-66  ; 0-9, A-F, a-f
*/