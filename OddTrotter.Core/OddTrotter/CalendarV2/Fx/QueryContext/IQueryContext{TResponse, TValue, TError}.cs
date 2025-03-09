namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryContext<out TResponse, out TValue, out TError>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ITask<IQueryResult<TResponse, TError>> Evaluate();
    }
}
