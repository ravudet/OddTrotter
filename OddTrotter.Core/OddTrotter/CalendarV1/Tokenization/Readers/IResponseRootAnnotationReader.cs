namespace OddTrotter.CalendarV1.Tokenization.Readers
{
    public interface IResponseRootAnnotationReader<out TNextReader> : IReader<IResponseRootAnnotationNameReader<TNextReader>>
    {
    }

    public interface IResponseRootAnnotationNameReader<out TNextReader> : IReader<IResponseRootAnnotationValueReader<TNextReader>, AnnotationName>
    {
    }

    public sealed class AnnotationName
    {
        internal AnnotationName(string value)
        {
            Value = value;
        }

        internal string Value { get; }
    }

    public interface IResponseRootAnnotationValueReader<out TNextReader> : IReader<TNextReader, AnnotationValue>
    {
    }

    public abstract class AnnotationValue
    {
        private AnnotationValue()
        {
        }

        internal sealed class Boolean : AnnotationValue
        {
            public Boolean(bool value)
            {
                Value = value;
            }

            public bool Value { get; }
        }

        internal sealed class Null : AnnotationValue
        {
            private Null()
            {
            }

            public static Null Instance { get; } = new Null();
        }

        internal sealed class Signed : AnnotationValue //// TODO i don't know if you need a better breakdown for numeric values than signed vs unsigned
        {
            public Signed(long value)
            {
                Value = value;
            }

            public long Value { get; }
        }

        internal sealed class Unsigned : AnnotationValue
        {
            public Unsigned(ulong value)
            {
                Value = value;
            }

            public ulong Value { get; }
        }

        internal sealed class String : AnnotationValue
        {
            public String(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }
    }
}
