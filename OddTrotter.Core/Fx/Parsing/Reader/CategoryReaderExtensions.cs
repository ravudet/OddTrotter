namespace Fx.Parsing.Reader
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class RefTask
    {
        public static CategoryReaderExtensions.RefTask<TResult, TResult> Completed<TResult>(TResult result)
            where TResult : allows ref struct
        {
            CategoryReaderExtensions.TryOperate<TResult, TResult> foo = static (TResult context, [MaybeNullWhen(false)] out TResult category, [MaybeNullWhen(true)] out ValueTask task) =>
            {
                category = context;
                task = default;
                return true;
            };

            return new CategoryReaderExtensions.RefTask<TResult, TResult>(foo, result);
        }
    }

    public static class CategoryReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TCategory>(this ICategoryReader<TCurrentReader, TCategory>? categoryReader, Context context, [MaybeNullWhen(false)] out TCategory category)
            where TCurrentReader : ICategoryReader<TCurrentReader, TCategory>
            where TCategory : allows ref struct
        {
            return TCurrentReader.TryMove(context, out category);
        }

        public static async ValueTask<TCategory> Move<TCurrentReader, TCategory>(this ICategoryReader<TCurrentReader, TCategory>? categoryReader, Context context)
            where TCurrentReader : ICategoryReader<TCurrentReader, TCategory>
        {
            TCategory? category;
            while (!categoryReader.TryMove(context, out category))
            {
                await context.Read().ConfigureAwait(false);
            }

            return category;
        }

        public delegate bool TryOperate<TIn, TOut>(TIn @in, [MaybeNullWhen(false)] out TOut @out, [MaybeNullWhen(true)] out ValueTask task)
            where TIn : allows ref struct
            where TOut : allows ref struct;

        public unsafe ref struct RefTask<TResult, TContext> //// TODO go ahead and create the awaitable types; you'll want to look at the `ieither` stuff too
            where TResult : allows ref struct
            where TContext : allows ref struct
        {
            private readonly TryOperate<TContext, TResult> tryOperate;
            private TContext context;
            private TResult result;
            private readonly ValueTask? task;

            public RefTask(TryOperate<TContext, TResult> tryOperate, TContext context)
            {
                this.tryOperate = tryOperate;
                this.context = context;

                if (this.tryOperate(this.context, out var result, out var task))
                {
                    this.result = result;
                }
                else
                {
                    this.result = default!; //// TODO refnullable? //// TODO use the correct naming
                    this.task = task;
                }
            }

            ////public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            public ConfiguredAwaitable.Awaiter GetAwaiter()
            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                return new ConfiguredAwaitable.Awaiter(this.tryOperate, (TContext*)Unsafe.AsPointer(ref this.context), (TResult*)Unsafe.AsPointer(ref this.result), this.task, false);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }

            public unsafe ref struct ConfiguredAwaitable
            {
                private readonly TryOperate<TContext, TResult> tryOperate;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                private readonly TContext* context;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                private TResult* result;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                private readonly ValueTask? task;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(TryOperate<TContext, TResult> tryOperate, ref TContext context, ref TResult result, ValueTask? task, bool continueOnCapturedContext)
                {
                    this.tryOperate = tryOperate;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    this.context = (TContext*)Unsafe.AsPointer(ref context);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    this.result = (TResult*)Unsafe.AsPointer(ref result);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    this.task = task;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public Awaiter GetAwaiter()
                {
                    return new Awaiter(this.tryOperate, this.context, this.result, this.task, this.continueOnCapturedContext);
                }

                public struct Awaiter : ICriticalNotifyCompletion
                {
                    private readonly TryOperate<TContext, TResult> tryOperate;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private readonly TContext* context;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private TResult* result;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private ValueTask? task;
                    private readonly bool continueOnCapturedContext;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    public Awaiter(TryOperate<TContext, TResult> tryOperate, TContext* context, TResult* result, ValueTask? task, bool continueOnCapturedContext)
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    {
                        this.tryOperate = tryOperate;
                        this.context = context;
                        this.result = result;
                        this.task = task;
                        this.continueOnCapturedContext = continueOnCapturedContext;
                    }

                    public bool IsCompleted
                    {
                        get
                        {
                            if (this.task == null)
                            {
                                return true;
                            }

                            if (!this.task.Value.IsCompleted)
                            {
                                return false;
                            }

                            if (this.tryOperate(Unsafe.AsRef<TContext>(this.context), out var result, out var task))
                            {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                                this.result = (TResult*)Unsafe.AsPointer(ref result);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                                //// TODO THIS DOESNT ACTUALLY WORK BECAUSE `result` LEAVES THE STACK FRAME IN A MOMENT
                                //// TODO it works for your current stuff because the readers are always null...
                                //// TODO is that actually true? isn't the result sometimes a category?
                                return true;
                            }
                            else
                            {
                                this.task = task;
                                return this.IsCompleted;
                            }
                        }
                    }

                    public void OnCompleted(Action continuation)
                    {
                        throw new NotImplementedException();
                    }

                    public void UnsafeOnCompleted(Action continuation)
                    {
                        throw new NotImplementedException();
                    }

                    public TResult GetResult()
                    {
                        return Unsafe.AsRef<TResult>(this.result);
                    }
                }
            }
        }
    }
}
