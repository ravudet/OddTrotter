/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either.Mixins;
    using Fx.Realizable;

    public static class EitherExtensions
    {
        private static TypeHolder<IEither<TLeft, TRight>, TLeft, TRight> TypeHolder<TLeft, TRight>(this IEither<TLeft, TRight> either)
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            return new TypeHolder<IEither<TLeft, TRight>, TLeft, TRight>(either);
        }

        private static Realizable<TResult> ToRealizable<TState, TResult>(TState state, Func<TState, TResult> func)
            where TState : allows ref struct
            where TResult : allows ref struct
        {
            TResult value;
            try
            {
                value = func(state);
            }
            catch (Exception exception)
            {
                return Realizable.FromException<TResult>(exception);
            }

            return Realizable.FromResult(value);
        }

        private static IEither<TLeft, TRight> ToEither<TLeft, TRight>(this RefEither<TLeft, TRight> either)
        {
            return either.Apply<RefEither<TLeft, TRight>, TLeft, TRight, Either<TLeft, TRight>>(
                left => new Either<TLeft, TRight>.Left(left),
                right => new Either<TLeft, TRight>.Right(right));
        }

        //// TODO instead of having all of these additional overloads, i think it ultimately makes more sense to let have the caller convert their `ieither` into a `refeither` and *then* they can call `select` (or convert their refeither into ieither etc); then you don't have to re-implement all of the variants, but with the "scoped" or "class" prefix //// TODo this *does* slightly reduce discoverability; for example, take the case where the caller has an `ieither` and wants delegates that return `ref struct`s; they need to be able to look at that compiler error and say "oh, my return type needs to be able to hold the `ref struct`s for the map, so my return type needs to be a `refeither`; and the `select` methods give me back what i started with, so i need to start with a `refeither` instead of an `ieither`; let me call `toref`

        //// TODO not a huge fan of this naming, or the fact that you needed to name the async variants "async"
        //// TODO can you call this `frameselect` or `scopedselect`
        public static RefEither<TLeftResult, TRightResult> ScopedSelect<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftResult : allows ref struct
            where TRightResult : allows ref struct
        {
            return either.TypeHolder().Select(leftMap, rightMap);
        }

        //// TODO better name; allocselect?
        /*public static IEither<TLeftResult, TRightResult> ClassSelect<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this RefEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
        {
        }*/

        private sealed class DeferredEither<TLeftSource, TRightSource, TLeftResult, TRightResult> : IEither<TLeftResult, TRightResult>
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftResult : allows ref struct
            where TRightResult : allows ref struct
        {
            private readonly IEither<TLeftSource, TRightSource> either;
            private readonly Func<TLeftSource, TLeftResult> leftMap;
            private readonly Func<TRightSource, TRightResult> rightMap;

            //// TODO needs a more appropriate name
            public DeferredEither(
                IEither<TLeftSource, TRightSource> either,
                Func<TLeftSource, TLeftResult> leftMap,
                Func<TRightSource, TRightResult> rightMap)
            {
                this.either = either;
                this.leftMap = leftMap;
                this.rightMap = rightMap;
            }

            public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<TLeftResult, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRightResult, TContext, TContinuable, TResult> rightMap, ref TContext context)
                where TResult : allows ref struct
                where TContext : allows ref struct
                where TContinuable : IContinuable<TResult>, allows ref struct
            {
                return Helper(leftMap, rightMap, ref context)
                    .ContinueWith(
                        source => source,
                        exception => throw exception,
                        cancelation => throw cancelation);
            }

            private TContinuable Helper<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<TLeftResult, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRightResult, TContext, TContinuable, TResult> rightMap, ref TContext context)
                where TResult : allows ref struct
                where TContext : allows ref struct
                where TContinuable : IContinuable<TResult>, allows ref struct
            {
                if (either.Decompose(out var left, out var right))
                {
                    try
                    {
                        return leftMap(this.leftMap(left), ref context);
                    }
                    catch (Exception exception)
                    {
                        throw new LeftMapException(exception);
                    }
                }
                else
                {
                    try
                    {
                        return rightMap(this.rightMap(right), ref context);
                    }
                    catch (Exception exception)
                    {
                        throw new RightMapException(exception);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TLeftSource"></typeparam>
        /// <typeparam name="TRightSource"></typeparam>
        /// <typeparam name="TLeftResult"></typeparam>
        /// <typeparam name="TRightResult"></typeparam>
        /// <param name="either"></param>
        /// <param name="leftMap"></param>
        /// <param name="rightMap"></param>
        /// <returns></returns>
        /// <exception cref="LeftMapException"></exception>
        /// <exception cref="RightMapException"></exception>
        public static IEither<TLeftResult, TRightResult> Select<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            /*where TLeftResult : allows ref struct
            where TRightResult : allows ref struct*/
        {
            /*//// TODO add tests for this have source and result types being ref structs
            var deferredEither = new DeferredEither<TLeftSource, TRightSource, TLeftResult, TRightResult>(either, leftMap, rightMap);

            deferredEither.Decompose(out var left, out var right); //// TODO are you happy with this?

            return deferredEither;*/

            return either.Select<IEither<TLeftSource, TRightSource>, TLeftSource, TRightSource, TLeftResult, TRightResult>(leftMap, rightMap).ToEither();
        }

        public static RefEither<TLeftResult, TRightResult> Select<TEither, TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this TypeHolder<TEither, TLeftSource, TRightSource> either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
            where TEither : IEither<TLeftSource, TRightSource>, allows ref struct
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftResult : allows ref struct
            where TRightResult : allows ref struct
        {
            //// TODO are you happy with this return type? can you do better?
            return either.Self.Select(leftMap, rightMap);
        }

        public static RefEither<TLeftResult, TRightResult> Select<TEither, TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this TEither either,
            Func<TLeftSource, TLeftResult> leftMap,
            Func<TRightSource, TRightResult> rightMap)
            where TEither : IEither<TLeftSource, TRightSource>, allows ref struct
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftResult : allows ref struct
            where TRightResult : allows ref struct
        {
            //// TODO are you happy with this return type? can you do better?
            var realizable = SelectAsync<TEither, TLeftSource, TRightSource, Realizable<TLeftResult>, TLeftResult, Realizable<TRightResult>, TRightResult>(
                either,
                left => ToRealizable(left, leftMap), //// TODO every non-async variant needs to use this adapter
                right => ToRealizable(right, rightMap));

            if (realizable.AsEither.Decompose(out var result, out var task))
            {
                return result;
            }
            else
            {
                return task.ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public static Realizable<IEither<TLeftResult, TRightResult>> SelectAsync<TLeftSource, TRightSource, TLeftResult, TRightResult>(
            this IEither<TLeftSource, TRightSource> either,
            Func<TLeftSource, IContinuable<TLeftResult>> leftMap,
            Func<TRightSource, IContinuable<TRightResult>> rightMap)
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
        {
            //// TODO make this note somewhere: this has to be called "async"; since the return type of the maps is an *interface*, then the caller's use of any concrete types will require an implicit conversion to find this overload over the normal `select` overload; because of this, the caller will not receive back a future<either> but instead will receive back and either<future>; this worked in *previous* implementations because the return type of the maps was `task` and not `itask`; while this will likely work for a large number of cases, it will destroy the naming convention if we want to support a caller who has implemented their own continuables; *their* callers would be able to use `select` for anything that is implemented with `task`, but would need to call `selectasync` (or lose type inference) when leveraging anything from the special continuable implementation

            return either
                .SelectAsync<IEither<TLeftSource, TRightSource>, TLeftSource, TRightSource, IContinuable<TLeftResult>, TLeftResult, IContinuable<TRightResult>, TRightResult>(leftMap, rightMap)
                .Select(
                    refEither => refEither.ToEither());
        }

        public static Realizable<RefEither<TLeftResult, TRightResult>> SelectAsync<TEither, TLeftSource, TRightSource, TLeftContinuable, TLeftResult, TRightContinuable, TRightResult>(
            this TEither either,
            Func<TLeftSource, TLeftContinuable> leftMap,
            Func<TRightSource, TRightContinuable> rightMap)
            where TEither : IEither<TLeftSource, TRightSource>, allows ref struct
            where TLeftSource : allows ref struct
            where TRightSource : allows ref struct
            where TLeftContinuable : IContinuable<TLeftResult>, allows ref struct
            where TLeftResult : allows ref struct
            where TRightContinuable : IContinuable<TRightResult>, allows ref struct
            where TRightResult : allows ref struct
        {
            return either.ApplyAsync<RefEither<TLeftResult, TRightResult>, bool, Realizable<RefEither<TLeftResult, TRightResult>>>(
                (TLeftSource left, ref bool context) =>
                    leftMap(left)
                    .ContinueWith(
                        result => new RefEither<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                (TRightSource right, ref bool context) =>
                    rightMap(right)
                    .ContinueWith(
                        result => new RefEither<TLeftResult, TRightResult>(result),
                        exception => throw exception,
                        canceled => throw canceled),
                ref Context);
        }



































        //// TODO this maybe should be documented somewhere, but you have these variants:
        //// {
        //// teither
        //// typeholder<teither>
        //// ieither
        //// +
        //// realizable<teither>
        //// realizable<typeholder<teither>>
        //// realizable<ieither>
        //// 
        //// CROSS
        //// 
        //// tcontinuable
        //// icontinuable
        //// realizable
        //// }
        ////
        //// TODO should you have "task" variants for `leftmap` and `rightmap`?
        //// TODO and maybe also for `either` (because the caller might be getting an `ieither` from an async method, not just from our extensions
        //// TODO "overload" means a change in the number of parameters, but "variant" means fiddling with the shape of each parameter

        public static Realizable<TResult> ApplyAsync<TLeft, TRight, TResult>(
            this Realizable<IEither<TLeft, TRight>> either,
            Func<TLeft, Realizable<TResult>> leftMap,
            Func<TRight, Realizable<TResult>> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.ApplyAsync<IEither<TLeft, TRight>, TLeft, TRight, TResult>(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this Realizable<TypeHolder<TEither, TLeft, TRight>> realizable,
            Func<TLeft, Realizable<TResult>> leftMap,
            Func<TRight, Realizable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return FromTypeHolder(realizable)
                .ApplyAsync(
                    leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this Realizable<TEither> realizable,
            Func<TLeft, Realizable<TResult>> leftMap,
            Func<TRight, Realizable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return realizable
                .ContinueWith(
                    either => either
                        .ApplyAsync(
                            leftMap,
                            rightMap),
                    _ => throw _,
                    _ => throw _)
                .Unwrap();
        }

        public static Realizable<TResult> ApplyAsync<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, Realizable<TResult>> leftMap,
            Func<TRight, Realizable<TResult>> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.TypeHolder().ApplyAsync(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this TypeHolder<TEither, TLeft, TRight> either,
            Func<TLeft, Realizable<TResult>> leftMap,
            Func<TRight, Realizable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.Self.ApplyAsync(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this TEither either,
            Func<TLeft, Realizable<TResult>> leftMap,
            Func<TRight, Realizable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.ApplyAsync<TEither, TLeft, TRight, Realizable<TResult>, TResult>(
                leftMap,
                rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TLeft, TRight, TResult>(
            this Realizable<IEither<TLeft, TRight>> either,
            Func<TLeft, IContinuable<TResult>> leftMap,
            Func<TRight, IContinuable<TResult>> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.ApplyAsync<IEither<TLeft, TRight>, TLeft, TRight, TResult>(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this Realizable<TypeHolder<TEither, TLeft, TRight>> realizable,
            Func<TLeft, IContinuable<TResult>> leftMap,
            Func<TRight, IContinuable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return FromTypeHolder(realizable)
                .ApplyAsync(
                    leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this Realizable<TEither> realizable,
            Func<TLeft, IContinuable<TResult>> leftMap,
            Func<TRight, IContinuable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return realizable
                .ContinueWith(
                    either => either
                        .ApplyAsync(
                            leftMap,
                            rightMap),
                    _ => throw _,
                    _ => throw _)
                .Unwrap();
        }

        public static Realizable<TResult> ApplyAsync<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, IContinuable<TResult>> leftMap,
            Func<TRight, IContinuable<TResult>> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.TypeHolder().ApplyAsync(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this TypeHolder<TEither, TLeft, TRight> either,
            Func<TLeft, IContinuable<TResult>> leftMap,
            Func<TRight, IContinuable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.Self.ApplyAsync(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TResult>(
            this TEither either,
            Func<TLeft, IContinuable<TResult>> leftMap,
            Func<TRight, IContinuable<TResult>> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.ApplyAsync<TEither, TLeft, TRight, IContinuable<TResult>, TResult>(
                leftMap,
                rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TLeft, TRight, TContinuable, TResult>(
            this Realizable<IEither<TLeft, TRight>> either,
            Func<TLeft, TContinuable> leftMap,
            Func<TRight, TContinuable> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
            where TResult : allows ref struct
        {
            return either.ApplyAsync<IEither<TLeft, TRight>, TLeft, TRight, TContinuable, TResult>(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TContinuable, TResult>(
            this Realizable<TypeHolder<TEither, TLeft, TRight>> realizable,
            Func<TLeft, TContinuable> leftMap,
            Func<TRight, TContinuable> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
            where TResult : allows ref struct
        {
            return FromTypeHolder(realizable)
                .ApplyAsync<TEither, TLeft, TRight, TContinuable, TResult>(
                    leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TContinuable, TResult>(
            this Realizable<TEither> realizable,
            Func<TLeft, TContinuable> leftMap,
            Func<TRight, TContinuable> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
            where TResult : allows ref struct
        {
            return realizable
                .ContinueWith(
                    either => either
                        .ApplyAsync<TEither, TLeft, TRight, TContinuable, TResult>(
                            leftMap,
                            rightMap),
                    _ => throw _,
                    _ => throw _)
                .Unwrap();
        }

        public static Realizable<TResult> ApplyAsync<TLeft, TRight, TContinuable, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TContinuable> leftMap,
            Func<TRight, TContinuable> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
            where TResult : allows ref struct
        {
            return either.ApplyAsync<IEither<TLeft, TRight>, TLeft, TRight, TContinuable, TResult>(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TContinuable, TResult>(
            this TypeHolder<TEither, TLeft, TRight> either,
            Func<TLeft, TContinuable> leftMap,
            Func<TRight, TContinuable> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
            where TResult : allows ref struct
        {
            return either.Self.ApplyAsync<TEither, TLeft, TRight, TContinuable, TResult>(leftMap, rightMap);
        }

        public static Realizable<TResult> ApplyAsync<TEither, TLeft, TRight, TContinuable, TResult>(
            this TEither either,
            Func<TLeft, TContinuable> leftMap,
            Func<TRight, TContinuable> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TContinuable : IContinuable<TResult>, allows ref struct
            where TResult : allows ref struct
        {
            return either.ApplyAsync<TResult, bool, TContinuable>(
                (TLeft left, ref bool context) => leftMap(left),
                (TRight right, ref bool context) => rightMap(right),
                ref Context);
        }





















































        public static TResult Apply<TLeft, TRight, TResult>(
            this IEither<TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.TypeHolder().Apply(leftMap, rightMap);
        }

        public static TResult Apply<TEither, TLeft, TRight, TResult>(
            this TypeHolder<TEither, TLeft, TRight> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.Self.Apply(leftMap, rightMap);
        }

        public static Realizable<TResult> Apply<TEither, TLeft, TRight, TResult>(
            this Realizable<TypeHolder<TEither, TLeft, TRight>> realizable,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return realizable.FromTypeHolder().Apply(leftMap, rightMap);
        }

        private static Realizable<TEither> FromTypeHolder<TEither, TLeft, TRight>(
            this Realizable<TypeHolder<TEither, TLeft, TRight>> realizable)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            //// TODO should this be called "async"?

            return realizable
                .AsEither
                .Apply(
                    realized => new Realizable<TEither>(realized.Self),
                    future => future.ContinueWith(
                        either => either.Self,
                        exception => throw exception,
                        canceled => throw canceled));
        }

        public static Realizable<TResult> Apply<TEither, TLeft, TRight, TResult>(
            this Realizable<TEither> realizable,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return realizable.ContinueWith(
                either => either.Apply(leftMap, rightMap),
                exception => throw exception,
                canceled => throw canceled);
        }

        public static TResult Apply<TEither, TLeft, TRight, TResult>(
            this TEither either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TEither : IEither<TLeft, TRight>, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            var future = either.ApplyAsync<TResult, bool, Realizable<TResult>>(
                (TLeft left, ref bool context) => ToRealizable(left, leftMap),
                (TRight right, ref bool context) => ToRealizable(right, rightMap),
                ref Context);

            if (future.AsEither.Decompose(out var result, out var task))
            {
                return result;
            }
            else
            {
                return task.ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        public static Realizable<TResult> Apply<TLeft, TRight, TResult>(
            this Realizable<IEither<TLeft, TRight>> either,
            Func<TLeft, TResult> leftMap,
            Func<TRight, TResult> rightMap)
            where TLeft : allows ref struct
            where TRight : allows ref struct
            where TResult : allows ref struct
        {
            return either.Apply<IEither<TLeft, TRight>, TLeft, TRight, TResult>(leftMap, rightMap);
        }


        public static bool Decompose<TEither, TLeft, TRight>(
            this TypeHolder<TEither, TLeft, TRight> either,
            [MaybeNullWhen(false)] out TLeft left,
            [MaybeNullWhen(true)] out TRight right)
            where TEither : IEither<TLeft, TRight>, ICastable, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            return Decompose(either.Self, out left, out right);
        }

        public static bool Decompose<TLeft, TRight>(
            this IEither<TLeft, TRight> either,
            [MaybeNullWhen(false)] out TLeft left,
            [MaybeNullWhen(true)] out TRight right)
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            return new DecomposeCastable<TLeft, TRight>(either).AsEither.Decompose(out left, out right);
        }

        private readonly ref struct DecomposeCastable<TLeft, TRight> : IEither<DecomposeCastable<TLeft, TRight>, TLeft, TRight>, ICastable
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            private readonly IEither<TLeft, TRight> either;

            public DecomposeCastable(IEither<TLeft, TRight> either)
            {
                this.either = either;
            }

            public TypeHolder<DecomposeCastable<TLeft, TRight>, TLeft, TRight> AsEither
            {
                get
                {
                    return new TypeHolder<DecomposeCastable<TLeft, TRight>, TLeft, TRight>(this);
                }
            }

            public Realizable<TResult> ApplyAsync<TResult, TContext, TContinuable>(AsyncRefContextualizedContinuableMap<TLeft, TContext, TContinuable, TResult> leftMap, AsyncRefContextualizedContinuableMap<TRight, TContext, TContinuable, TResult> rightMap, ref TContext context)
                where TResult : allows ref struct
                where TContext : allows ref struct
                where TContinuable : IContinuable<TResult>, allows ref struct
            {
                return this.either.ApplyAsync(leftMap, rightMap, ref context);
            }

            public bool TryCast<TCasted>([MaybeNullWhen(false)] out TCasted casted)
                where TCasted : struct, allows ref struct
            {
                if (this.either is IDecomposeMixin<IEither<TLeft, TRight>, TLeft, TRight>)
                {
                    if (DecomposeMixin.TryCreate(
                        this,
                        (DecomposeCastable<TLeft, TRight> either, [MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right) => ((IDecomposeMixin<IEither<TLeft, TRight>, TLeft, TRight>)either.either).Decompose(out left, out right),
                        out casted))
                    {
                        return true;
                    }
                }

                casted = default;
                return false;
            }
        }

        public static bool Decompose<TEither, TLeft, TRight>(
            this TEither either,
            [MaybeNullWhen(false)] out TLeft left,
            [MaybeNullWhen(true)] out TRight right)
            where TEither : IEither<TLeft, TRight>, ICastable, allows ref struct
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            if (either.TryCast<DecomposeMixin<TEither, TLeft, TRight>>(out var casted))
            {
                return casted.Decompose(out left, out right);
            }

            var context = new DecomposeContext<TLeft, TRight>();
            var result = either.ApplyAsync<bool, DecomposeContext<TLeft, TRight>, Realizable<bool>>(
                (TLeft left, ref DecomposeContext<TLeft, TRight> context) =>
                {
                    context.Left = left;
                    context.IsLeft = true;
                    return new Realizable<bool>(true);
                },
                (TRight right, ref DecomposeContext<TLeft, TRight> context) =>
                {
                    context.Right = right;
                    context.IsLeft = false;
                    return new Realizable<bool>(true);
                },
                ref context);

            left = context.Left;
            right = context.Right;
            return context.IsLeft;
        }

        private ref struct DecomposeContext<TLeft, TRight>
            where TLeft : allows ref struct
            where TRight : allows ref struct
        {
            public bool IsLeft { get; set; }
            public TLeft Left { get; set; }
            public TRight Right { get; set; }
        }

        private static bool Context = false;
    }
}
