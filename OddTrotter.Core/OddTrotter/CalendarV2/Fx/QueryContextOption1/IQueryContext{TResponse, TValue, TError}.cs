namespace Fx.QueryContextOption1
{
    using System.Threading.Tasks;

    using Fx.QueryContext;

    public interface IQueryContext<out TResponse, out TValue, out TError>
    {
        ITask<IQueryResult<TResponse, TError>> Evaluate();
    }
}
