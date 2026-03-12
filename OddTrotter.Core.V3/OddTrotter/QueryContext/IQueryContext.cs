/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryContext<out TClientValue, out TDataStoreValue, out TError> //// TODO document why there is no "synchronous" query context or query result
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EvaluationException{TError}">Thrown if the initial call to the backing data store resulting in a <typeparamref name="TError"/></exception>
        ITask<IQueryResult<TClientValue, TError>> Evaluate();
    }
}
