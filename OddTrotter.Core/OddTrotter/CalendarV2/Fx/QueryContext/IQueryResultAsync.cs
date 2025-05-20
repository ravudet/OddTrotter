/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryResultAsync<out TValue, out TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        ITask<IQueryResultNodeAsync<TValue, TError>> GetNodes();
    }
}
