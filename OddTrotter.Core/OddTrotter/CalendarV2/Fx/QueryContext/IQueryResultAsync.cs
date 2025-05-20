namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IQueryResultAsync<out TValue, out TError>
    {
        ITask<IQueryResultNodeAsync<TValue, TError>> GetNodes();
    }
}
