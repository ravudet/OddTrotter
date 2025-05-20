/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.QueryContext
{
    using System;
    using System.Threading.Tasks;

    using Fx.Either;

    internal sealed class MockElementAsync : IElementAsync<string, Exception>
    {
        private readonly IQueryResultNodeAsync<string, Exception> next;

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        public MockElementAsync(string value)
            : this(
                value,
                Either
                    .Left<IElementAsync<string, Exception>>()
                    .Right(
                        Either
                            .Left<IError<Exception>>()
                            .Right(
                                MockEmpty.Instance))
                    .ToQueryResultNodeAsync())
        {
        }

        /// <summary>
        /// placeholder
        /// </summary>
        /// <param name="value"></param>
        /// <param name="next"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="next"/> is <see langword="null"/></exception>
        public MockElementAsync(string value, IQueryResultNodeAsync<string, Exception> next)
        {
            ArgumentNullException.ThrowIfNull(next);

            this.Value = value;
            this.next = next;
        }

        /// <inheritdoc/>
        public string Value { get; }

        /// <inheritdoc/>
        public async ITask<IQueryResultNodeAsync<string, Exception>> Next()
        {
            //// TODO you should use `await` here
            return await Task.FromResult(this.next).ConfigureAwait(false);
        }
    }
}
