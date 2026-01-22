/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using Fx.Either;

    public interface IQueryResultNodeAsync<out TValue, out TError>
        : IEither<IElementAsync<TValue, TError>, IEither<IError<TError>, IEmpty>>
    {
    }
}
