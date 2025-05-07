/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using Fx.Either;

    public interface IQueryResultNode<TValue, TError> 
        : IEither<IElement<TValue, TError>, IEither<IError<TError>, IEmpty>>
    {
    }
}
