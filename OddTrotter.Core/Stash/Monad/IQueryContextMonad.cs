using Fx.QueryContext;

namespace Stash.Monad
{
    /// <summary>
    /// TODO i'm skipping this for now; the monad + mixin pattern was very useful for enumerables because it allowed "extensions" on top of existing monads and/or mixins which could improve the behavior of those existing types; however, because there is no "default" implementation for the mixins, and because query contexts are really generating query strings to a backing data store, there's not really a "general purpose" implementation of any particular mixin; further, if i've done the mixins correctly, adding monads should be externally extensible, and if i've not done mixins correctly, the mixins, right now, are mere "guidance" for query context implementers of things that their query contexts might be able to do; they should be able to be easily thrown away and new interfaces created that allow for the creation of monads if needed
    /// </summary>
    /// <typeparam name="TQueryContextMonad"></typeparam>
    /// <typeparam name="TQueryContext"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TError"></typeparam>
    public interface IQueryContextMonad<TQueryContextMonad, TQueryContext, TResponse, TValue, TError> :
        IQueryContext<TResponse, TValue, TError>
        where TQueryContextMonad : IQueryContextMonad<TQueryContextMonad, TQueryContext, TResponse, TValue, TError>
        where TQueryContext : IQueryContext<TResponse, TValue, TError>
    {
        TQueryContext Source { get; }

        /*QueryContextUnit<TQueryContext2, TResponse2, TValue2, TError2> Unit<TQueryContext2, TResponse2, TValue2, TError2>() where TQueryContext2 : IQueryContext<TResponse2, TValue2, TError2>;*/

        //// TODO is the issue you're running into here that mixins need more than a monad, they need a kernel?
        QueryContextUnit<TQueryContextMonad, TQueryContext, TResponse, TValue, TError> Unit(); //// TODO by not have the above generic type parameters, you lose out on being able to do selects; selects as linq presents them (which we are an analog to), are an entirely in-memory concept and so should applied to the query result; however, if a client is able specify the selects the query context selecting tvalue (keeping tresponse the same), the the querycontext *could* be able to only retrieve those properties from the database that the client has actually asked for; so, selects on querycontexts would actually be very useful to reduced bandwidth on the wire; and beacuse we don't have the generic type paramters on the unit, the returned monad must have the same type parameters as the source monad, resulting in the same tvalue, meaning it can't have been selected

        TQueryContextMonad Self { get; }
    }
}
