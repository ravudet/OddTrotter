namespace Fx.Json
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Fx.Parsing.Reader;

    public sealed class ValueReader<TNextReader> : ICategoryReader<ValueReader<TNextReader>, ValueCategory<TNextReader>>
    {
        public static bool TryMove(Context context, [MaybeNullWhen(false)] out ValueCategory<TNextReader> category)
        {
            if (context.CurrentByteIndex >= context.ValidBytes)
            {
                category = default;
                return false;
            }

            if (context.ValidBytes == 0)
            {
                throw new InvalidPayloadException("TODO invalid JSON");
            }

            switch ((char)context.Buffer[context.CurrentByteIndex])
            {
                case 'f':
                    category = ValueCategory<TNextReader>.False();
                    return true;
                case 'n':
                    category = ValueCategory<TNextReader>.Null();
                    return true;
                case 't':
                    category = ValueCategory<TNextReader>.True();
                    return true;
                case '{':
                    category = ValueCategory<TNextReader>.Object();
                    return true;
                case '[':
                    category = ValueCategory<TNextReader>.Array();
                    return true;
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    category = ValueCategory<TNextReader>.Number();
                    return true;
                case '"':
                    category = ValueCategory<TNextReader>.String();
                    return true;
                default:
                    throw new InvalidPayloadException("tODO invalid JSON");
            }
        }
    }
}
