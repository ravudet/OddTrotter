using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public interface IValueReader<out TSelf, TNextReader, TValue>
        where TSelf : IValueReader<TSelf, TNextReader, TValue>
        where TValue : allows ref struct
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="nextReader"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidPayloadException">Thrown if the payload in <paramref name="context"/> is not the format that the reader implementation understands</exception>
        static abstract bool TryMove(Context context, out TNextReader? nextReader, [MaybeNullWhen(false)] out TValue value); //// TODO you really have to jump through some hoops to make this covariant; do you want to?
    }
}
