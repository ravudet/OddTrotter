/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using Fx.Either;

    public interface IQueryResultNode<out TValue, out TError>
        : IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>> //// TODO should the right side be a new interface called `iterminal`?
    {
    }
}
