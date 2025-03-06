namespace Fx.QueryContextOption1
{
    using Fx.QueryContext;

    public interface IQueryContext<TValue, TError> : IQueryContext<TValue, TValue, TError> //// TODO is this overload really helpful? maybe, but do you want to set a precedent that you need to do that for the mixins too? or maybe mixins are "advanced" enough that you should expect the dev to be able to handle it
    {
    }
}
