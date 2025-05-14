namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IElementAsync<out TValue, out TError>
    {
        TValue Value { get; }

        ITask<IQueryResultNodeAsync<TValue, TError>> NextAsync();
    }
}
