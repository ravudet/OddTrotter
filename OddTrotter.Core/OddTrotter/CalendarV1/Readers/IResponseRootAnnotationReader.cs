namespace OddTrotter.CalendarV1.Readers
{
    public interface IResponseRootAnnotationReader<out TNextReader> : IReader<IResponseRootAnnotationNameReader<TNextReader>>
    {
    }

    public interface IResponseRootAnnotationNameReader<out TNextReader> : IReader<IResponseRootAnnotationValueReader<TNextReader>, AnnotationName>
    {
    }

    public sealed class AnnotationName
    {
        private AnnotationName()
        {
        }
    }

    public interface IResponseRootAnnotationValueReader<out TNextReader> : IReader<TNextReader, AnnotationValue>
    {
    }

    public sealed class AnnotationValue
    {
        private AnnotationValue()
        {
            //// TODO this might be a DU between strings and each of the different primitives; see if you can find the answer in the standard //// TODO actually, it seems to be *any* JSON token, including an object, but the standard doesn't actually make this specific
        }
    }
}
