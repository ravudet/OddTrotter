namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryContext<out TResponse, out TValue, out TError>
    {
        ITask<IQueryResult<TResponse, TError>> Evaluate();
    }
}
