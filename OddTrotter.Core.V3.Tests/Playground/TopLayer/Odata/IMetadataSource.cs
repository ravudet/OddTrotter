namespace Playground.TopLayer.Odata
{
    using System;
    using System.Threading.Tasks;

    //// TODO you are here
    //// TODO finish this file, including todos


    public static class Playground
    {
        public static void DoWork<TSchemaVersion>(IMetadataSource<TSchemaVersion> source)
        {
            source.Get2().Format<MetadataDto.Json>().Evaluate();

            source.Get2().Evaluate().GetAwaiter().GetResult().Apply(
                _ => "asdf",
                _ => "asdf",
                _ => "asdf");
        }
    }


    public interface IMetadataSource<TSchemaVersion>
    {
        IMetadataContext<TSchemaVersion> Get();

        IMetadataContext2<TSchemaVersion, IMetadataDto> Get2();

        //// TODO other verbs
    }

    public interface IMetadataDto
    {
        internal void CantImplement();

        TResult Apply<TResult>(
            Func<MetadataDto.Xml, TResult> xml,
            Func<MetadataDto.Json, TResult> json,
            Func<MetadataDto.Unknown, TResult> unknown);
    }

    public interface IMetadataContext2<TSchemaVersion, TFormat>
        where TFormat : class, IMetadataDto
    {
        ITask<TFormat> Evaluate();

        IMetadataContext2<TSchemaVersion, TFormat2> Format<TFormat2>() where TFormat2 : MetadataDto, IMetadataDto;
    }



    public interface IBaseMetadataContext<TSchemaVersion>
    {
        ITask<MetadataDto> Evaluate(); //// TODO can't just return the dto, need to have control information, headers, etc.

        //// TODO request headers
    }


    public interface IMetadataContext<TSchemaVersion> : IBaseMetadataContext<TSchemaVersion>
    {
        ISchemaVersionMetadataContext<TSchemaVersion> SchemaVersion(TSchemaVersion schemaVersion); //// TODO lots of implications, read through this: https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html#sec_SystemQueryOptionschemaversion

        IFormatMetadataContext<TSchemaVersion, TFormat> Format<TFormat>() where TFormat : MetadataDto;
    }

    public interface IFormatMetadataContext<TSchemaVersion, TFormat> : IBaseMetadataContext<TSchemaVersion> 
        where TFormat : MetadataDto //// TODO is there a way to disallow the base type? //// TODO and `unknown`
    {
        new ITask<TFormat> Evaluate();

        IBaseMetadataContext<TSchemaVersion> SchemaVersion(TSchemaVersion schemaVersion); //// TODO lots of implications, read through this: https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html#sec_SystemQueryOptionschemaversion
    }

    public interface ISchemaVersionMetadataContext<TSchemaVersion> : IBaseMetadataContext<TSchemaVersion>
    {
        IBaseMetadataContext<TSchemaVersion> Format<TFormat>() where TFormat : MetadataDto;
    }

    public abstract class MetadataDto
    {
        //// TODO

        private MetadataDto()
        {
        }

        protected internal abstract TResult Apply<TResult>(
            Func<MetadataDto.Xml, TResult> xml, 
            Func<MetadataDto.Json, TResult> json,
            Func<MetadataDto.Unknown, TResult> unknown);

        public sealed class Xml : MetadataDto, IMetadataDto
        {
            protected internal override TResult Apply<TResult>(
                Func<Xml, TResult> xml,
                Func<Json, TResult> json,
                Func<MetadataDto.Unknown, TResult> unknown)
            {
                return xml(this);
            }
        }

        public sealed class Json : MetadataDto, IMetadataDto
        {
            protected internal override TResult Apply<TResult>(
                Func<Xml, TResult> xml,
                Func<Json, TResult> json,
                Func<MetadataDto.Unknown, TResult> unknown)
            {
                return json(this);
            }
        }

        public sealed class Unknown : MetadataDto
        {
            protected internal override TResult Apply<TResult>(
                Func<Xml, TResult> xml,
                Func<Json, TResult> json,
                Func<MetadataDto.Unknown, TResult> unknown)
            {
                return unknown(this);
            }
        }
    }

    public static class MetadataDtoExtensions
    {
        public static TResult Apply2<TResult>(this MetadataDto metadataDto, Func<MetadataDto.Xml, TResult> xml, Func<MetadataDto.Json, TResult> json, Func<MetadataDto.Unknown, TResult> unknown)
        {
            //// TODO does this work? external consumers can only get this extension method, so if we add a new derived type, then this method can still be found with binary compatibility, and this method implementation will be updated in the new binary to call the correct `metadatadto.apply` method; we would also expose a *new* extension with the new parameter

            return metadataDto.Apply(xml, json, unknown);
        }
    }
}
