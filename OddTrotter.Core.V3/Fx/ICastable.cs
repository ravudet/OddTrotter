/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx
{
    using System.Diagnostics.CodeAnalysis;
    
    public interface ICastable
    {
        bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted) where TCasted : struct, allows ref struct; //// note: `tcasted` *must* be a struct for mixin purposes when ref structs are allowed because we can't actually return an interface that also encapsulates the thing being cast
    }
}
