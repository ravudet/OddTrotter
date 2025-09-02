namespace OddTrotter.CalendarV1.Tokenization.Readers
{
    using System;

    public interface ICustomHeaderReader<out TNextReader> : IReader<ICustomHeaderFieldNameReader<TNextReader>>
    {
    }

    public interface ICustomHeaderFieldNameReader<out TNextReader> : IReader<ICustomHeaderFieldValueReader<TNextReader>, CustomHeaderFieldName>
    {
    }

    public sealed class CustomHeaderFieldName
    {
        private CustomHeaderFieldName()
        {
        }
    }

    public interface ICustomHeaderFieldValueReader<out TNextReader> : IReader<ICustomHeaderFieldValueElementReader<TNextReader>>
    {
    }

    public interface ICustomHeaderFieldValueElementReader<out TNextReader> : IReader<ICustomHeaderFieldValueElementToken<TNextReader>>
    {
    }

    public interface ICustomHeaderFieldValueElementToken<out TNextReader>
    {
        TResult Apply<TResult>(
            Func<ICustomHeaderFieldContentReader<TNextReader>, TResult> customHeaderFieldContentReader,
            Func<ICustomHeaderLwsReader<TNextReader>, TResult> customHeaderLwsReader,
            Func<TNextReader, TResult> nextReader);
    }

    public interface ICustomHeaderFieldContentReader<out TNextReader> : IReader<ICustomHeaderFieldValueElementReader<TNextReader>, CustomHeaderFieldContent>
    {
    }

    public sealed class CustomHeaderFieldContent
    {
        private CustomHeaderFieldContent()
        {
        }
    }

    public interface ICustomHeaderLwsReader<out TNextReader> : IReader<ICustomHeaderFieldValueElementReader<TNextReader>, CustomHeaderLws>
    {
    }

    public sealed class CustomHeaderLws
    {
        private CustomHeaderLws()
        {
        }
    }
}
