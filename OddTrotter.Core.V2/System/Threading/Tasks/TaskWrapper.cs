/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    public sealed class TaskWrapper<T> : ITask<T>
    {
        private readonly Task<T> task;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is <see langword="null"/></exception>
        public TaskWrapper(Task<T> task)
        {
            ArgumentNullException.ThrowIfNull(task);

            this.task = task;
        }

        /// <inheritdoc/>
        public ITaskAwaiter<T> GetAwaiter()
        {
            return new TaskAwaiterWrapper<T>(this.task.GetAwaiter());
        }

        /// <inheritdoc/>
        public IConfiguredAwaitable<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            return new ConfiguredAwaitableWrapper<T>(this.task.ConfigureAwait(continueOnCapturedContext));
        }

		public ITask<TResult> ContinueWith<TResult>(Func<ITask<T>,TResult> continuationFunction)
			where TResult : allows ref struct
		{
			//// TODO this could be an extension method
			return new ContinueWithTask<T, TResult>(this, continuationFunction);
		}
    }
	
	public sealed class ContinueWithTask<TSource, TResult> : ITask<TResult> where TSource : allows ref struct where TResult : allows ref struct
	{
		private readonly ITask<TSource> source;
		private readonly Func<ITask<TSource>, TResult> continuationFunction;
		
		public ContinueWithTask(ITask<TSource> source, Func<ITask<TSource>, TResult> continuationFunction)
		{
			this.source = source;
			this.continuationFunction = continuationFunction;
		}

		/// <inheritdoc/>
		public ITaskAwaiter<TResult> GetAwaiter()
		{
			return new ContinueWithTaskAwaiter(this.source, this.source.GetAwaiter(), this.continuationFunction);
		}
		
		private sealed class ContinueWithTaskAwaiter : ITaskAwaiter<TResult>
		{
			private readonly ITask<TSource> source;
			private readonly ITaskAwaiter<TSource> taskAwaiter;
			private readonly Func<ITask<TSource>, TResult> continuationFunction;

			/// <summary>
			/// placeholder
			/// </summary>
			/// <param name="taskAwaiter"></param>
			public ContinueWithTaskAwaiter(ITask<TSource> source, ITaskAwaiter<TSource> taskAwaiter, Func<ITask<TSource>, TResult> continuationFunction)
			{
				this.source = source;
				this.taskAwaiter = taskAwaiter;
				this.continuationFunction = continuationFunction;
			}

			/// <inheritdoc/>
			public bool IsCompleted
			{
				get
				{
					return this.taskAwaiter.IsCompleted;
				}
			}

			/// <inheritdoc/>
			public TResult GetResult()
			{
				return this.continuationFunction(this.source);
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				ArgumentNullException.ThrowIfNull(continuation);

				this.taskAwaiter.OnCompleted(continuation);
			}

			/// <inheritdoc/>
			public void UnsafeOnCompleted(Action continuation)
			{
				ArgumentNullException.ThrowIfNull(continuation);

				this.taskAwaiter.UnsafeOnCompleted(continuation);
			}
		}

		/// <inheritdoc/>
		public IConfiguredAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
		{
			throw new Exception("TODO");
		}

		public ITask<TResult2> ContinueWith<TResult2>(Func<ITask<TResult>,TResult2> continuationFunction)
			where TResult2 : allows ref struct
		{
			return new ContinueWithTask<TResult, TResult2>(this, continuationFunction);
		}
	}
}
