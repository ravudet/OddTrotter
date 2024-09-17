﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.OdataPocRoot.Odata.Odata
{
    using System;
    using Fx.OdataPocRoot.Odata.UriExpressionNodes.Expand;
    using Fx.OdataPocRoot.Odata.UriExpressionNodes.Filter;
    using Fx.OdataPocRoot.Odata.UriExpressionNodes.Select;

    public abstract class OdataRequest
    {
        private OdataRequest()
        {
        }

        public sealed class GetCollection : OdataRequest //// TODO would having something like get, and then a derived trype for collection + derived type for insteance cost more than it's worth?
        {
            public GetCollection(RelativeUri uri, Expand? expand, Filter? filter, Select? select)
            {
                Uri = uri;
                Expand = expand;
                Filter = filter;
                Select = select;
            }

            public RelativeUri Uri { get; }

            public Expand? Expand { get; }

            public Filter? Filter { get; }

            public Select? Select { get; }

            //// TODO add other query options here
        }

        public sealed class GetInstance : OdataRequest
        {
            public GetInstance(RelativeUri uri, Expand? expand, Select? select)
            {
                Uri = uri;
                this.Expand = expand;
                Select = select;
            }

            public RelativeUri Uri { get; }

            public Expand? Expand { get; }

            public Select? Select { get; }

            //// TODO add other query options here
        }
    }
}