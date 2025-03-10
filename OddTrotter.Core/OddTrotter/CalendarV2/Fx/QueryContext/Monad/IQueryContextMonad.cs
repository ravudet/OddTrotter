namespace Fx.QueryContext.Monad
{
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
