﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.OdataPocRoot.V2.Odata.UriQueryOptions.Filter.Visitors
{
    using System;
    using global::System.Text;

    using Fx.OdataPocRoot.V2.Odata.UriQueryOptions.Filter;

    public sealed class CastExprToStringVisitor : CastExpr.Visitor<Void, StringBuilder>
    {
    }
}