using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public interface IMoveReader<out TSelf, TNextReader>
        where TSelf : IMoveReader<TSelf, TNextReader>
    {
        static abstract bool MoveTry(Context context, out TNextReader? nextReader);
    }
}
