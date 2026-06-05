using System.Diagnostics.CodeAnalysis;

namespace Fx.Parsing.Reader
{
    public interface IMoveReader<out TSelf, out TNextReader>
        where TSelf : IMoveReader<TSelf, TNextReader>
    {
        static abstract TNextReader? MoveTry(Context context, out bool moved); //// TODO does this covariance thing make it slower?
    }
}
