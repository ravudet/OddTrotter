﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.OdataPocRoot.V2.Odata.UriQueryOptions.Filter
{
    public abstract class NotExpr
    {
        private NotExpr()
        {
            //// TODO FEATURE GAP: finish this
        }

        protected abstract TResult Accept<TResult, TContext>(Visitor<TResult, TContext> visitor, TContext context);

        public abstract class Visitor<TResult, TContext>
        {
            public TResult Traverse(NotExpr node, TContext context)
            {
                return node.Accept(this, context);
            }
        }
    }
}