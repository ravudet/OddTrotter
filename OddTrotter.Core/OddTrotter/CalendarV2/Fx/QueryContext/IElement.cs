/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fx.QueryContext
{
    public interface IElement<TValue, TError>
    {
        /// <summary>
        /// placeholder
        /// </summary>
        TValue Value { get; }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method should not throw. In the event of an error, a <see cref="IError{TError}"/> should be returned instead.
        /// </remarks>
        IQueryResultNode<TValue, TError> Next();

        public async Task<IQueryResultNode<TValue, TError>> NextAsync()
        {
            return await Task.FromResult(this.Next()).ConfigureAwait(false);
        }
    }
}
