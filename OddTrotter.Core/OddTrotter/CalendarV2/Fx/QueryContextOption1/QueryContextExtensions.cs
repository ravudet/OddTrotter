namespace Fx.QueryContextOption1
{
    using Fx.QueryContext;
    using Fx.QueryContextOption1.Mixins;
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;


    public static class QueryContextExtensions
    {
        /*public static TQueryContext Where<TQueryContext, TResponse, TValue, TError>(this TQueryContext queryContext, System.Linq.Expressions.Expression<Func<TValue, bool>> predicate)
            where TQueryContext : Mixins.IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            return queryContext.Where(predicate);
        }*/

        public sealed class CalendarContext : IWhereQueryContextMixin<string, string, Exception, CalendarContext>
        {
            public Task<IQueryResult<string, Exception>> Evaluate()
            {
                throw new NotImplementedException();
            }

            public CalendarContext Where(Expression<Func<string, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            ITask<IQueryResult<string, Exception>> IQueryContext<string, string, Exception>.Evaluate()
            {
                throw new NotImplementedException();
            }
        }

        public interface IQueryContextMonad<TQueryContext, TResponse, TValue, TError> : IQueryContext<TResponse, TValue, TError> where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            TQueryContext Source { get; }

            Unit<TQueryContext2, TResponse2, TValue2, TError2> Unit<TQueryContext2, TResponse2, TValue2, TError2>() where TQueryContext2 : IQueryContext<TResponse2, TValue2, TError2>;
        }

        public delegate IQueryContextMonad<TQueryContext, TResponse, TValue, TError> Unit<TQueryContext, TResponse, TValue, TError>(TQueryContext queryContext) where TQueryContext : IQueryContext<TResponse, TValue, TError>;

        public static IQueryContextMonad<TQueryContext, TResponse, TValue, TError> Where<TQueryContext, TResponse, TValue, TError>(this IQueryContextMonad<TQueryContext, TResponse, TValue, TError> monad, Expression<Func<TValue, bool>> predicate)
            //where TQueryContext : IQueryContext<TResponse, TValue, TError>
            where TQueryContext : IWhereQueryContextMixin<TResponse, TValue, TError, TQueryContext>
        {
            return monad.Unit<TQueryContext, TResponse, TValue, TError>()(monad.Source.Where(predicate));

            throw new NotImplementedException();
        }

        public sealed class RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError> : IQueryContextMonad<TQueryContext, TResponse, TValue, TError>, IWhereQueryContextMixin<TResponse, TValue, TError, RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>>

            where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            public RavudetImprovedExtensions(TQueryContext queryContext)
            {
                this.Source = queryContext;
            }

            public TQueryContext Source { get; }

            public Task<IQueryResult<TResponse, TError>> Evaluate()
            {
                throw new NotImplementedException();
            }

            public Unit<TQueryContext2, TResponse2, TValue2, TError2> Unit<TQueryContext2, TResponse2, TValue2, TError2>() where TQueryContext2 : IQueryContext<TResponse2, TValue2, TError2>
            {
                ////return context => new Bar<TQueryContext2, TResponse2, TValue2, TError2>(context);
                return AddRavudetImprovedExtensions<TQueryContext2, TResponse2, TValue2, TError2>;
            }

            public RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError> Where(Expression<Func<TValue, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            ITask<IQueryResult<TResponse, TError>> IQueryContext<TResponse, TValue, TError>.Evaluate()
            {
                throw new NotImplementedException();
            }
        }

        public static RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError> AddRavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>(this TQueryContext queryContext)
            where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            return new RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>(queryContext);
        }

        public sealed class CalendarContext2 : IQueryContext<string, string, Exception>
        {
            public Task<IQueryResult<string, Exception>> Evaluate()
            {
                throw new NotImplementedException();
            }

            ITask<IQueryResult<string, Exception>> IQueryContext<string, string, Exception>.Evaluate()
            {
                throw new NotImplementedException();
            }
        }

        public static void DoWork(CalendarContext calendarContext, CalendarContext2 calendarContext2)
        {
            ////foo.Where(val => true);

            var ravudetImprovedExtensions = calendarContext.AddRavudetImprovedExtensions<CalendarContext, string, string, Exception>();
            var result = ravudetImprovedExtensions.Where(val => true).Where(val => true);

            var ravudetImprovedExtensions2 = calendarContext2.AddRavudetImprovedExtensions<CalendarContext2, string, string, Exception>();
            var results2 = ravudetImprovedExtensions2.Where(val => true);

            calendarContext = calendarContext.Where(val => true).Where(val => true);
        }

        public sealed class CalendarContext3 : IWhereQueryContextMixin<string, string, Exception, CalendarContext3>, IHasTypeParameters<string, string, Exception>
        {
            public static ITypeParameters<string, string, Exception> TypeParameters => throw new NotImplementedException();

            public Task<IQueryResult<string, Exception>> Evaluate()
            {
                throw new NotImplementedException();
            }

            public CalendarContext3 Where(Expression<Func<string, bool>> predicate)
            {
                //// TODO i think the mixin should call this `whereimpl` and you have the `where` extension on the mixin to support wrapping the result in a monad where applicable (otherwise the implementer of the monad will need to constantly wrap their results in the monad)
                throw new NotImplementedException();
            }

            ITask<IQueryResult<string, Exception>> IQueryContext<string, string, Exception>.Evaluate()
            {
                throw new NotImplementedException();
            }
        }

        public static void TypeParametersDriver(CalendarContext calendarContext, CalendarContext3 calendarContext3, CalendarContext4 calendarContext4)
        {
            var ravudetImprovedExtensions = calendarContext.AddRavudetImprovedExtensions(Of.Type<string>(), Of.Type<string>(), Of.Type<Exception>());

            var ravudetImprovedExtensions2 = calendarContext3.AddRavudetImprovedExtensions(CalendarContext3.TypeParameters);


            var ravudetImprovedExtensions3 = calendarContext4.AddRavudetImprovedExtensions();
            ravudetImprovedExtensions3.Where(valu => true);
        }

        public static RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError> AddRavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>(this TQueryContext queryContext, Type<TResponse> response, Type<TValue> value, Type<TError> error)
            where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            return AddRavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>(queryContext);
        }

        public static RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError> AddRavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>(this TQueryContext queryContext, ITypeParameters<TResponse, TValue, TError> typeParameters)
            where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            return AddRavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>(queryContext);
        }

        public static RavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError> AddRavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>(this IHasTypeParameters2<TQueryContext, TResponse, TValue, TError> queryContext) where TQueryContext : IQueryContext<TResponse, TValue, TError>
        {
            return queryContext.Instance.AddRavudetImprovedExtensions<TQueryContext, TResponse, TValue, TError>();
        }

        public sealed class CalendarContext4 : IWhereQueryContextMixin<string, string, Exception, CalendarContext4>, IHasTypeParameters2<CalendarContext4, string, string, Exception>
        {
            public CalendarContext4 Instance
            {
                get
                {
                    return this;
                }
            }

            public Task<IQueryResult<string, Exception>> Evaluate()
            {
                throw new NotImplementedException();
            }

            public CalendarContext4 Where(Expression<Func<string, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            ITask<IQueryResult<string, Exception>> IQueryContext<string, string, Exception>.Evaluate()
            {
                throw new NotImplementedException();
            }
        }

        public interface IHasTypeParameters2<TInstance, T1, T2, T3>
        {
            TInstance Instance { get; }
        }

        public interface IHasTypeParameters<T1, T2, T3>
        {
            static abstract ITypeParameters<T1, T2, T3> TypeParameters { get; } //// TODO is this being static *really* any better than be an intance property? //// TODO it actually might if you put it on `iquerycontext` instead of `buzz`
        }

        public interface ITypeParameters<T1, T2, T3>
        {
        }

        public static class Of
        {
            public static Type<T> Type<T>()
            {
                return QueryContextExtensions.Type<T>.Instance;
            }
        }

        public sealed class Type<T>
        {
            private Type()
            {
            }

            public static Type<T> Instance { get; } = new Type<T>();
        }
    }

    public static class NewEitherPlayground
    {
        //// TODO can you somehow make all of this solve your visitor explosion problem? where you have a selectasync overload (for example) that takes in a TEither : IEitherAsync and returns a TEitherResult?
        public static void Driver()
        {
            var either = new Either<string, Exception>.Left("asdf");
            either.Select<Either<string, Exception>, string, Exception, Either<string, Nothing>, string, Nothing>(
                val => val, 
                error => new Nothing(),
                EitherFactory<string, Nothing>.Instance);

            either.Select2<Either<string, Exception>, string, Exception, Either<string, Nothing>, string, Nothing>(
                val => val,
                error => new Nothing());

            ////either.Select3(val => val, error => new Nothing());

            /*either.Select4<Either<string, Exception>, string, Exception, EitherFactory<string, Nothing>,  Either<string, Nothing>, string, Nothing>(val => val, error => new Nothing(), EitherFactory.Create<string, Nothing>);*/

            /*Parameters.Create(either).AddLeftSelector(val => val).AddRightSelector(error => new Nothing()).AddFactory(EitherFactory.Generate);

            either.Select4(val => val, error => new Nothing(), EitherFactory.Create);*/
        }

        public static class Parameters
        {
            public static Builder<TEitherSource, TLeftSource, TRightSource> Create<TEitherSource, TLeftSource, TRightSource>(IHasGenericTypeParameters<TEitherSource, TLeftSource, TRightSource> either)
                where TEitherSource : IEither<TLeftSource, TRightSource>
            {
                return Create<TEitherSource, TLeftSource, TRightSource>(either.Self);
            }

            public static Builder<TEitherSource, TLeftSource, TRightSource> Create<TEitherSource, TLeftSource, TRightSource>(TEitherSource either)
                where TEitherSource : IEither<TLeftSource, TRightSource>
            {
                return new Builder<TEitherSource, TLeftSource, TRightSource>(either);
            }

            public sealed class Builder<TEitherSource, TLeftSource, TRightSource>
            {
                private readonly TEitherSource either;

                public Builder(TEitherSource either)
                {
                    this.either = either;
                }

                public Builder2<TLeftResult> AddLeftSelector<TLeftResult>(Func<TLeftSource, TLeftResult> leftSelector)
                {
                    return new Builder2<TLeftResult>(this.either, leftSelector);
                }

                public sealed class Builder2<TLeftResult>
                {
                    private readonly TEitherSource either;
                    private readonly Func<TLeftSource, TLeftResult> leftSelector;

                    public Builder2(TEitherSource either, Func<TLeftSource, TLeftResult> leftSelector)
                    {
                        this.either = either;
                        this.leftSelector = leftSelector;
                    }

                    public Builder3<TRightResult> AddRightSelector<TRightResult>(Func<TRightSource, TRightResult> rightSelector)
                    {
                        return new Builder3<TRightResult>(this.either, this.leftSelector, rightSelector);
                    }

                    public sealed class Builder3<TRightResult>
                    {
                        private readonly TEitherSource either;
                        private readonly Func<TLeftSource, TLeftResult> leftSelector;
                        private readonly Func<TRightSource, TRightResult> rightSelector;

                        public Builder3(TEitherSource either, Func<TLeftSource, TLeftResult> leftSelector, Func<TRightSource, TRightResult> rightSelector)
                        {
                            this.either = either;
                            this.leftSelector = leftSelector;
                            this.rightSelector = rightSelector;
                        }

                        public Builder4<TFactory> AddFactory<TFactory, TEitherResult>(Func<IHasGenericTypeParameters<TFactory, TEitherResult, TLeftResult, TRightResult>> factory)
                        {
                            return new Builder4<TFactory>(factory.Invoke().Self);
                        }

                        public sealed class Builder4<TFactory>
                        {
                            public Builder4(TFactory factory)
                            {
                            }
                        }
                    }
                }
            }
        }

        public sealed class FuncWrapper<T> : IFunc<T>, IHasGenericTypeParameters<FuncWrapper<T>, T>
        {
            private readonly Func<T> func;

            public FuncWrapper(Func<T> func)
            {
                this.func = func;
            }

            public FuncWrapper<T> Self => this;

            public T Invoke()
            {
                return this.func();
            }
        }

        public interface IFunc<T>
        {
            T Invoke();
        }

        public sealed class Parameters<TEitherSource, TLeftSource, TRightSource, TEitherResult, TLeftResult, TRightResult>
        {

        }

        public static TEitherResult Select4<TEitherSource, TLeftSource, TRightSource, TFactory, TEitherResult, TLeftResult, TRightResult>(this IHasGenericTypeParameters<TEitherSource, TLeftSource, TRightSource> source, Func<TLeftSource, TLeftResult> leftSelector, Func<TRightSource, TRightResult> rightSelector, Func<IHasGenericTypeParameters<TFactory, TEitherResult, TLeftResult, TRightResult>> factory)
            where TEitherSource : IEither<TLeftSource, TRightSource>
            where TEitherResult : IEither<TLeftResult, TRightResult>
            where TFactory : IEitherFactory<TEitherResult, TLeftResult, TRightResult>
        {
            return Select4<TEitherSource, TLeftSource, TRightSource, TFactory, TEitherResult, TLeftResult, TRightResult>(source.Self, leftSelector, rightSelector, factory().Self);
        }

        public static TEitherResult Select4<TEitherSource, TLeftSource, TRightSource, TFactory, TEitherResult, TLeftResult, TRightResult>(this TEitherSource source, Func<TLeftSource, TLeftResult> leftSelector, Func<TRightSource, TRightResult> rightSelector, TFactory factory)
            where TEitherSource : IEither<TLeftSource, TRightSource>
            where TEitherResult : IEither<TLeftResult, TRightResult>
            where TFactory : IEitherFactory<TEitherResult, TLeftResult, TRightResult>
        {
            return source.Apply(
                left => factory.MakeLeft(leftSelector(left)),
                right => factory.MakeRight(rightSelector(right)));
        }

        /*public static TEitherResult Select3<TEitherSource, TLeftSource, TRightSource, TEitherResult, TLeftResult, TRightResult>(this IHasGenericTypeParameters<TEitherSource, TLeftSource, TRightSource, TLeftResult, TRightResult> source, Func<TLeftSource, TLeftResult> leftSelector, Func<TRightSource, TRightResult> rightSelector)
            where TEitherSource : IEither<TLeftSource, TRightSource>
            where TEitherResult : IEither<TLeftResult, TRightResult>, IEitherFactory2<TEitherResult, TLeftResult, TRightResult>
        {
            return source.Self.Select3<TEitherSource, TLeftSource, TRightSource, TEitherResult, TLeftResult, TRightResult>(leftSelector, rightSelector);
        }

        public static TEitherResult Select3<TEitherSource, TLeftSource, TRightSource, TEitherResult, TLeftResult, TRightResult>(this TEitherSource source, Func<TLeftSource, TLeftResult> leftSelector, Func<TRightSource, TRightResult> rightSelector)
            where TEitherSource : IEither<TLeftSource, TRightSource>
            where TEitherResult : IEither<TLeftResult, TRightResult>, IEitherFactory2<TEitherResult, TLeftResult, TRightResult>
        {
            return source.Apply(
                left => TEitherResult.MakeLeft(leftSelector(left)),
                right => TEitherResult.MakeRight(rightSelector(right)));
        }*/

        public static TEitherResult Select2<TEitherSource, TLeftSource, TRightSource, TEitherResult, TLeftResult, TRightResult>(this TEitherSource source, Func<TLeftSource, TLeftResult> leftSelector, Func<TRightSource, TRightResult> rightSelector)
            where TEitherSource : IEither<TLeftSource, TRightSource>
            where TEitherResult : IEither<TLeftResult, TRightResult>, IEitherFactory2<TEitherResult, TLeftResult, TRightResult>
        {
            return source.Apply(
                left => TEitherResult.MakeLeft(leftSelector(left)),
                right => TEitherResult.MakeRight(rightSelector(right)));
        }

        public static TEitherResult Select<TEitherSource, TLeftSource, TRightSource, TEitherResult, TLeftResult, TRightResult>(this TEitherSource source, Func<TLeftSource, TLeftResult> leftSelector, Func<TRightSource, TRightResult> rightSelector,
            IEitherFactory<TEitherResult, TLeftResult, TRightResult> factory)
            where TEitherSource : IEither<TLeftSource, TRightSource>
            where TEitherResult : IEither<TLeftResult, TRightResult>
        {
            return source.Apply(
                left => factory.MakeLeft(leftSelector(left)),
                right => factory.MakeRight(rightSelector(right)));
        }

        public interface IEither<out TLeft, out TRight>
        {
            TResult Apply<TResult>(Func<TLeft, TResult> leftMap, Func<TRight, TResult> rightMap);
        }

        public static class EitherFactory
        {
            public static Func<EitherFactory<TLeft, TRight>> Generate<TLeft, TRight>()
            {
                return new Func<EitherFactory<TLeft, TRight>>(Create<TLeft, TRight>);
            }

            public static EitherFactory<TLeft, TRight> Create<TLeft, TRight>()
            {
                return EitherFactory<TLeft, TRight>.Instance;
            }
        }

        public interface IEitherFactory<out TEither, TLeft, TRight> where TEither : IEither<TLeft, TRight>
        {
            TEither MakeLeft(TLeft left);

            TEither MakeRight(TRight right);
        }

        public sealed class EitherFactory<TLeft, TRight> : IEitherFactory<Either<TLeft, TRight>, TLeft, TRight>, IHasGenericTypeParameters<EitherFactory<TLeft, TRight>, TLeft, TRight>, IHasGenericTypeParameters<EitherFactory<TLeft, TRight>, Either<TLeft, TRight>, TLeft, TRight>
        {
            private EitherFactory()
            {
            }

            public static EitherFactory<TLeft, TRight> Instance { get; } = new EitherFactory<TLeft, TRight>();

            public EitherFactory<TLeft, TRight> Self => this;

            public Either<TLeft, TRight> MakeLeft(TLeft left)
            {
                return new Either<TLeft, TRight>.Left(left);
            }

            public Either<TLeft, TRight> MakeRight(TRight right)
            {
                return new Either<TLeft, TRight>.Right(right);
            }
        }

        public interface IEitherFactory2<out TEither, TLeft, TRight> where TEither : IEither<TLeft, TRight>
        {
            static abstract TEither MakeLeft(TLeft left);

            static abstract TEither MakeRight(TRight right);
        }

        public interface IEitherFactory3<out TEither, TLeft, TRight> where TEither : IEither<TLeft, TRight>
        {
            TEither MakeLeft(TLeft left);

            TEither MakeRight(TRight right);
        }

        public sealed class EitherFactory3<TEither, TLeft, TRight> : IEitherFactory3<TEither, TLeft, TRight> where TEither : IEither<TLeft, TRight>
        {
            public TEither MakeLeft(TLeft left)
            {
                throw new NotImplementedException();
            }

            public TEither MakeRight(TRight right)
            {
                throw new NotImplementedException();
            }
        }

        public abstract class Either2<TLeft, TRight, TLeft2, TRight2> : IEither<TLeft, TRight>, IEitherFactory2<Either2<TLeft2, TRight2, TLeft, TRight>, TLeft2, TRight2>, IHasGenericTypeParameters<Either2<TLeft, TRight, TLeft2, TRight2>, TLeft, TRight>
        {
            private Either2()
            {
            }

            public Either2<TLeft, TRight, TLeft2, TRight2> Self
            {
                get
                {
                    return this;
                }
            }

            public static Either2<TLeft2, TRight2, TLeft, TRight> MakeLeft(TLeft2 left)
            {
                return new Either2<TLeft2, TRight2, TLeft, TRight>.Left(left);
            }

            public static Either2<TLeft2, TRight2, TLeft, TRight> MakeRight(TRight2 right)
            {
                return new Either2<TLeft2, TRight2, TLeft, TRight>.Right(right);
            }

            public TResult Apply<TResult>(Func<TLeft, TResult> leftMap, Func<TRight, TResult> rightMap)
            {
                if (this is Left left)
                {
                    return leftMap(left.Value);
                }
                else if (this is Right right)
                {
                    return rightMap(right.Value);
                }

                throw new NotImplementedException("TODO use a visitor");
            }

            public sealed class Left : Either2<TLeft, TRight, TLeft2, TRight2>
            {
                public Left(TLeft value)
                {
                    Value = value;
                }

                public TLeft Value { get; }
            }

            public sealed class Right : Either2<TLeft, TRight, TLeft2, TRight2>
            {
                public Right(TRight value)
                {
                    Value = value;
                }

                public TRight Value { get; }
            }
        }

        public abstract class Either<TLeft, TRight> : IEither<TLeft, TRight>, IEitherFactory2<Either<TLeft, TRight>, TLeft, TRight>, IHasGenericTypeParameters<Either<TLeft, TRight>, TLeft, TRight>
        {
            private Either()
            {
            }

            public Either<TLeft, TRight> Self
            {
                get
                {
                    return this;
                }
            }

            public static Either<TLeft, TRight> MakeLeft(TLeft left)
            {
                return new Either<TLeft, TRight>.Left(left);
            }

            public static Either<TLeft, TRight> MakeRight(TRight right)
            {
                return new Either<TLeft, TRight>.Right(right);
            }

            public TResult Apply<TResult>(Func<TLeft, TResult> leftMap, Func<TRight, TResult> rightMap)
            {
                if (this is Left left)
                {
                    return leftMap(left.Value);
                }
                else if (this is Right right)
                {
                    return rightMap(right.Value);
                }

                throw new NotImplementedException("TODO use a visitor");
            }

            public sealed class Left : Either<TLeft, TRight>
            {
                public Left(TLeft value)
                {
                    Value = value;
                }

                public TLeft Value { get; }
            }

            public sealed class Right : Either<TLeft, TRight>
            {
                public Right(TRight value)
                {
                    Value = value;
                }

                public TRight Value { get; }
            }
        }

        public interface IHasGenericTypeParameters<TSelf, T1, T2>
        {
            TSelf Self { get; }
        }

        public interface IHasGenericTypeParameters<TSelf, T1, T2, T3>
        {
            TSelf Self { get; }
        }

        public interface IHasGenericTypeParameters<TSelf, T1>
        {
            TSelf Self { get; }
        }
    }
}
