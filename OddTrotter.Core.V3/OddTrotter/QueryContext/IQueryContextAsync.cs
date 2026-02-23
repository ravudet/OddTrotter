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
        ITask<IQueryResultAsync<TClientValue, TError>> Evaluate();
    }
}
