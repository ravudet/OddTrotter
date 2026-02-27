/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryContextAsync<out TClientValue, out TDataStoreValue, out TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EvaluationException{TError}">Thrown if the initial call to the backing data store resulting in a <typeparamref name="TError"/></exception>
        ITask<IQueryResultAsync<TClientValue, TError>> Evaluate();
    }
}
