/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    public interface IQueryResult<out TValue, out TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        IQueryResultNode<TValue, TError> Nodes { get; }
    }
}
