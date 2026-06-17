namespace Playground.TopLayer.Odata
{
    using System;
    using System.ComponentModel.Design;
    using System.Formats.Tar;
    using System.Runtime.InteropServices.Marshalling;
    using System.Threading.Tasks;
    using System.Xml;

    //// TODO you are here
    //// TODO finish this file, including todos


    public static class Playground
    {
        public static async Task DoWork(IMetadataSource<string> source)
        {

            var formatted = source.Get().Format<MetadataDto.Known.Xml>();
            IResponse<MetadataDto.Known.Xml> evaluated = await formatted.Evaluate();

            var versioned = formatted.SchemaVersion("asdf");
            IResponse<MetadataDto.Known.Xml> versionedEvaluation = await versioned.Evaluate();

            var versionedFirst = source.Get().SchemaVersion("asf").Format<MetadataDto.Known.Xml>();
            IResponse<MetadataDto.Known.Xml> versionedFirstEvalation = await versionedFirst.Evaluate();


            // source.Get().SchemaVersion("asf").Format<MetadataDto.Known>(); // correctly doesn't compile; the caller should be telling us exactly what format they expect
            // source.Get().SchemaVersion("asf").Format<MetadataDto.Unknown>(); // correctly doesn't compile; the caller shouldn't take any action on the "client side" if they don't know what format they expect
        }

        public static void FormatTest(
            // IMetadataContext2<string, MetadataDto.Unknown> first, // correctly doesn't compile; you shouldn't be able to know that the type is unknwon while still on the "client side"
            // IMetadataContext2<string, MetadataDto.Known> second, // correctly doesn't compile; if you know the type, then the explicit type should be used
            IMetadataContext2<string, MetadataDto.Known.Xml> third,
            IMetadataContext2<string, MetadataDto.Known.Json> fourth)
        {
        }
    }


    public interface IMetadataSource<TSchemaVersion>
    {
        IMetadataContext1<TSchemaVersion> Get();
        
        //// TODO i'm torn about putting other verbs here; they don't mean anything for `$metadata`, but do you want to always include all verbs and let the caller get an error anyway? ///// TODO i want there to be a layer *somewhere* (and maybe *this* isn't that layer) that represents what odata actually looks like when the rules are followed
    }

    public interface IMetadataContext1<TSchemaVersion>
        : IMetadataContext1<IMetadataContext1<TSchemaVersion>, TSchemaVersion>
    {
    }

    public interface IMetadataContext1<out TMetadataContext, TSchemaVersion>
        where TMetadataContext : IMetadataContext1<TMetadataContext, TSchemaVersion>
    {

        //// TODO request headers //// TODO maybe it makes the most sense to just have 3 "sets" of interfaces, 1 for each portion of the URL (i.e. source is segments, context is query options, and "something else" is headers)

        //// TODO how to have custom query options
        //// TODO how to have custom headers

        ITask<IResponse<MetadataDto>> Evaluate(); //// TODO can't just return the dto, need to have control information, headers, etc.

        TMetadataContext SchemaVersion(TSchemaVersion schemaVersion); //// TODO strongly type schema version

        IMetadataContext2<TSchemaVersion, TFormat> Format<TFormat>()
            where TFormat : MetadataDto.Known, IMetadataFormat;
    }

    public interface IMetadataContext2<TSchemaVersion, out TFormat>
        : IMetadataContext2<IMetadataContext2<TSchemaVersion, TFormat>, TSchemaVersion, TFormat>
        where TFormat : MetadataDto.Known, IMetadataFormat
    {
    }

    public interface IMetadataContext2<out TMetadataContext, TSchemaVersion, out TFormat>
        : IMetadataContext1<TMetadataContext, TSchemaVersion>
        where TMetadataContext : IMetadataContext2<TMetadataContext, TSchemaVersion, TFormat>
        where TFormat : MetadataDto.Known, IMetadataFormat
    {
        new ITask<IResponse<TFormat>> Evaluate(); //// TODO can't just return the dto, need to have control information, headers, etc.
    }

    public interface IMetadataFormat
    {
        internal void CantImplement();
    }

    public abstract class MetadataDto
    {
        private MetadataDto()
        {
        }

        internal abstract TResult Visit<TResult>(Func<Known, TResult> known, Func<Unknown, TResult> unknown);


        //// TODO have apply methods
        
        
        public abstract class Known : MetadataDto
        {
            private Known()
            {
            }

            internal abstract TResult Visit<TResult>(Func<Json, TResult> json, Func<Xml, TResult> xml, Func<NewType, TResult> newType);

            internal override TResult Visit<TResult>(Func<Known, TResult> known, Func<Unknown, TResult> unknown)
            {
                return known(this);
            }

            public sealed class Json : Known, IMetadataFormat
            {
                internal override TResult Visit<TResult>(Func<Json, TResult> json, Func<Xml, TResult> xml, Func<NewType, TResult> newType)
                {
                    throw new NotImplementedException();
                }

                void IMetadataFormat.CantImplement()
                {
                    throw new NotImplementedException();
                }
            }

            public sealed class Xml : Known, IMetadataFormat
            {
                internal override TResult Visit<TResult>(Func<Json, TResult> json, Func<Xml, TResult> xml, Func<NewType, TResult> newType)
                {
                    throw new NotImplementedException();
                }

                void IMetadataFormat.CantImplement()
                {
                    throw new NotImplementedException();
                }
            }

            public sealed class NewType : Known, IMetadataFormat
            {
                internal override TResult Visit<TResult>(Func<Json, TResult> json, Func<Xml, TResult> xml, Func<NewType, TResult> newType)
                {
                    throw new NotImplementedException();
                }

                void IMetadataFormat.CantImplement()
                {
                    throw new NotImplementedException();
                }
            }
        }

        public sealed class Unknown : MetadataDto
        {
            internal override TResult Visit<TResult>(Func<Known, TResult> known, Func<Unknown, TResult> unknown)
            {
                return unknown(this);
            }
        }
    }

    public static class MetadataDtoExtensions
    {
        public static TResult Apply<TResult>(
            this MetadataDto metadataDto, 
            Func<MetadataDto.Known.Xml, TResult> xml, 
            Func<MetadataDto.Known.Json, TResult> json, 
            Func<MetadataDto.Unknown, TResult> unknown)
        {
            //// TODO does this work? external consumers can only get this extension method, so if we add a new derived type, then this method can still be found with binary compatibility, and this method implementation will be updated in the new binary to call the correct `metadatadto.apply` method; we would also expose a *new* extension with the new parameter

            //// TODO you are here
            //// TODO make sure to use useful comments
            return metadataDto.Visit(
                known => known.Visit(json, xml, newType => ),
                unknown);
        }
    }
}
