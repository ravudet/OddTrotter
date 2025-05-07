/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryContext<TClientValue, TDataStoreValue, TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        ITask<IQueryResult<TClientValue, TError>> Evaluate();
    }
}
