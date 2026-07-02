namespace Fx.Parsing.Reader
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using OddTrotter.CalendarV1.Tokenization.Json;

    public static class CategoryReaderExtensions
    {
        public static bool TryMove<TCurrentReader, TCategory>(this ICategoryReader<TCurrentReader, TCategory>? categoryReader, Context context, [MaybeNullWhen(false)] out TCategory category)
            where TCurrentReader : ICategoryReader<TCurrentReader, TCategory>
            where TCategory : allows ref struct
        {
            return TCurrentReader.TryMove(context, out category);
        }

        public static RefTask<TCategory, Context> Move<TCurrentReader, TCategory>(this ICategoryReader<TCurrentReader, TCategory>? categoryReader, Context context)
            where TCurrentReader : ICategoryReader<TCurrentReader, TCategory>
            where TCategory : allows ref struct
        {
            TryOperate<Context, TCategory> foo = (Context context, [MaybeNullWhen(false)] out TCategory category, [MaybeNullWhen(true)] out ValueTask task) =>
            {
                if (categoryReader.TryMove(context, out category))
                {
                    task = default;
                    return true;
                }
                else
                {
                    task = context.Read();
                    return false;
                }
            };

            return new RefTask<TCategory, Context>(foo, context);
        }

        public delegate bool TryOperate<TIn, TOut>(TIn @in, [MaybeNullWhen(false)] out TOut @out, [MaybeNullWhen(true)] out ValueTask task)
            where TOut : allows ref struct;

        public ref struct RefTask<TResult, TContext> //// TODO go ahead and create the awaitable types; you'll want to look at the `ieither` stuff too
            where TResult : allows ref struct
        {
            private readonly TryOperate<TContext, TResult> tryOperate;
            private readonly TContext context;
            private readonly TResult result;
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

            public ConfiguredAwaitable ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(this.tryOperate,  this.context, this.result, this.task, continueOnCapturedContext);
            }

            public ref struct ConfiguredAwaitable
            {
                private readonly TryOperate<TContext, TResult> tryOperate;
                private readonly TContext context;
                private TResult result;
                private readonly ValueTask? task;
                private readonly bool continueOnCapturedContext;

                public ConfiguredAwaitable(TryOperate<TContext, TResult> tryOperate, TContext context, TResult result, ValueTask? task, bool continueOnCapturedContext)
                {
                    this.tryOperate = tryOperate;
                    this.context = context;
                    this.result = result;
                    this.task = task;
                    this.continueOnCapturedContext = continueOnCapturedContext;
                }

                public Awaiter GetAwaiter()
                {
                    return new Awaiter(this.tryOperate, this.context, ref this.result, this.task, this.continueOnCapturedContext);
                }

                public unsafe struct Awaiter : ICriticalNotifyCompletion
                {
                    private readonly TryOperate<TContext, TResult> tryOperate;
                    private readonly TContext context;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private TResult* result;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    private ValueTask? task;
                    private readonly bool continueOnCapturedContext;

                    public Awaiter(TryOperate<TContext, TResult> tryOperate, TContext context, ref TResult result, ValueTask? task, bool continueOnCapturedContext)
                    {
                        this.tryOperate = tryOperate;
                        this.context = context;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                        this.result = (TResult*)Unsafe.AsPointer(ref result);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
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

                            if (this.tryOperate(this.context, out var result, out var task))
                            {
                                this.result = Unsafe.AsPointer(ref result);
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
                        if (this.task == null)
                        {
                            return Unsafe.AsRef<TResult>(this.result);
                        }
                        else
                        {

                        }
                    }
                }
        }
    }
}
