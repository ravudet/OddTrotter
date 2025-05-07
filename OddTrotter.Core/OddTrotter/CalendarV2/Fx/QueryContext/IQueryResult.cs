/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    public interface IQueryResult<TValue, TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        IQueryResultNode<TValue, TError> Nodes { get; }
    }
}
