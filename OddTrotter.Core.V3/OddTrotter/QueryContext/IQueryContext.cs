/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryContext<out TClientValue, out TDataStoreValue, out TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        ITask<IQueryResult<TClientValue, TError>> Evaluate();
    }
}
