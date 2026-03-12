/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System.Threading.Tasks;

    public interface IElement<out TValue, out TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        TValue Value { get; }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <remarks>
        /// This method should not throw. In the event of an error, a <see cref="IError{TError}"/> should be returned instead.
        /// </remarks>
        ITask<IQueryResultNode<TValue, TError>> Next();
    }
}
