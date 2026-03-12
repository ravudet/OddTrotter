/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryResult<out TValue, out TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        ITask<IQueryResultNode<TValue, TError>> GetNodes();
    }
}
