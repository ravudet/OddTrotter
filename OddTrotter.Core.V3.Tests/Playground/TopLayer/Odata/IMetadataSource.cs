namespace Playground.TopLayer.Odata
{
    using System;
    using System.Formats.Tar;
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
    }

    public interface IMetadataContext1<TSchemaVersion>
        : IMetadataContext1<IMetadataContext1<TSchemaVersion>, TSchemaVersion>
    {
    }

    public interface IMetadataContext1<out TMetadataContext, TSchemaVersion>
        where TMetadataContext : IMetadataContext1<TMetadataContext, TSchemaVersion>
    {
        ITask<IResponse<MetadataDto>> Evaluate();

        TMetadataContext SchemaVersion(TSchemaVersion schemaVersion);

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
        new ITask<IResponse<TFormat>> Evaluate();
    }

    /*public interface IMetadataContext<out TSchemaVersion, TFormat> //// TODO you shouldn't be able to get a context that has metadatadto.unknown as the type; since the context is "client-side" we can't know yet that the response will be `unknown`
        where TFormat : MetadataFormat
    {
        ITask<IResponse<TFormat>> Evaluate();

        IMetadataContext<TSchemaVersion, TConcreteFormat> Format<TConcreteFormat>()
            where TConcreteFormat : MetadataFormat, IMetadataFormat;
    }*/

    public interface IMetadataFormat
    {
    }

    public abstract class MetadataDto
    {
        public abstract class Known : MetadataDto
        {
            public sealed class Json : Known, IMetadataFormat
            {
            }

            public sealed class Xml : Known, IMetadataFormat
            {
            }
        }

        public sealed class Unknown : MetadataDto
        {
        }
    }

    /*public interface IMetadataSource<TSchemaVersion>
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
    }*/
}
