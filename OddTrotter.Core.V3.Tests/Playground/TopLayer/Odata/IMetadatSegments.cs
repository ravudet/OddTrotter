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
        public static async Task DoWork(IMetadatSegments<string> source)
        {

            var formatted = source.Verbs().Get().Options().Format<MetadataDto.Known.Xml>();
            IResponse<MetadataDto.Known.Xml> evaluated = await formatted.Headers().Evaluate();

            var versioned = formatted.SchemaVersion("asdf");
            IResponse<MetadataDto.Known.Xml> versionedEvaluation = await versioned.Headers().Evaluate();

            var versionedFirst = source.Verbs().Get().Options().SchemaVersion("asf").Format<MetadataDto.Known.Xml>();
            IResponse<MetadataDto.Known.Xml> versionedFirstEvalation = await versionedFirst.Headers().Evaluate();

            // source.Verbs().Get().Options().SchemaVersion("asf").Format<MetadataDto.Known>(); // correctly doesn't compile; the caller should be telling us exactly what format they expect
            // source.Verbs().Get().Options().SchemaVersion("asf").Format<MetadataDto.Unknown>(); // correctly doesn't compile; the caller shouldn't take any action on the "client side" if they don't know what format they expect

            IResponse<MetadataDto> unknownEvaluation = await source.Verbs().Get().Options().Headers().Evaluate();
        }

        public static void FormatTest(
            // IFormattedMetadataOptions<string, MetadataDto.Unknown> first, // correctly doesn't compile; you shouldn't be able to know that the type is unknwon while still on the "client side"
            // IFormattedMetadataOptions<string, MetadataDto.Known> second, // correctly doesn't compile; if you know the type, then the explicit type should be used
            IFormattedMetadataOptions<string, MetadataDto.Known.Xml> third,
            IFormattedMetadataOptions<string, MetadataDto.Known.Json> fourth,

            // IFormattedMetadataHeaders<MetadataDto.Unknown> fifth, // correctly doesn't compile; you shouldn't be able to know that the type is unknwon while still on the "client side"
            // IFormattedMetadataHeaders<MetadataDto.Known> sixth, // correctly doesn't compile; if you know the type, then the explicit type should be used
            IFormattedMetadataHeaders<MetadataDto.Known.Xml> seventh,
            IFormattedMetadataHeaders<MetadataDto.Known.Json> eighth)
        {
        }
    }




    //// TODO consider:
    ////    1. to know the legal verbs, you have to know the segments; for example, you can't do `post` to a single-valued property
    ////    2. to know the legal query options, you have to know the segments; for example, you can't do `$filter` to a single-valued property
    ////    3. to know the legal query options, you have to know the verb; for example, you can't use `$index` with a `get` request
    //// TODO what else?


    public interface IMetadatSegments<TSchemaVersion>
    {
        IMetadataVerbs<TSchemaVersion> Verbs();
    }






    public interface IMetadataVerbs<TSchemaVersion>
    {

        //// TODO i'm torn about putting other verbs here; they don't mean anything for `$metadata`, but do you want to always include all verbs and let the caller get an error anyway? ///// TODO i want there to be a layer *somewhere* (and maybe *this* isn't that layer) that represents what odata actually looks like when the rules are followed

        IMetadataVerbs<TSchemaVersion> Get(); //// TODO should this just return the options directly? it kind of breaks the larger pattern, but once you've selected the verb, there's nothing else to really do

        IMetadataOptions<TSchemaVersion> Options();
    }







    public interface IMetadataOptions<TSchemaVersion>
        : IMetadataOptions<IMetadataOptions<TSchemaVersion>, TSchemaVersion>
    {
    }

    public interface IMetadataOptions<out TMetadataOptions, TSchemaVersion>
        where TMetadataOptions : IMetadataOptions<TMetadataOptions, TSchemaVersion>
    {

        IMetadataHeaders Headers();

        //// TODO how to have custom query options
        //// TODO how to have custom headers

        TMetadataOptions SchemaVersion(TSchemaVersion schemaVersion); //// TODO strongly type schema version

        IFormattedMetadataOptions<TSchemaVersion, TFormat> Format<TFormat>()
            where TFormat : MetadataDto.Known, IMetadataFormat;
    }

    public interface IFormattedMetadataOptions<TSchemaVersion, out TFormat>
        : IFormattedMetadataOptions<IFormattedMetadataOptions<TSchemaVersion, TFormat>, TSchemaVersion, TFormat>
        where TFormat : MetadataDto.Known, IMetadataFormat
    {
    }

    public interface IFormattedMetadataOptions<out TMetadataOptions, TSchemaVersion, out TFormat>
        : IMetadataOptions<TMetadataOptions, TSchemaVersion>
        where TMetadataOptions : IFormattedMetadataOptions<TMetadataOptions, TSchemaVersion, TFormat>
        where TFormat : MetadataDto.Known, IMetadataFormat
    {
        new IFormattedMetadataHeaders<TFormat> Headers();
    }





    public interface IMetadataHeaders : IMetadataHeaders<IMetadataHeaders>
    {
    }

    public interface IMetadataHeaders<out TMetadataHeaders>
        where TMetadataHeaders : IMetadataHeaders<TMetadataHeaders>
    {
        ITask<IResponse<MetadataDto>> Evaluate(); //// TODO can't just return the dto, need to have control information, headers, etc.
    }

    public interface IFormattedMetadataHeaders<out TFormat> : IFormattedMetadataHeaders<IFormattedMetadataHeaders<TFormat>, TFormat>
        where TFormat : MetadataDto.Known, IMetadataFormat
    {
    }

    public interface IFormattedMetadataHeaders<out TMetadataHeaders, out TFormat> : IMetadataHeaders<TMetadataHeaders>
        where TMetadataHeaders : IFormattedMetadataHeaders<TMetadataHeaders, TFormat>
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
                    return json(this);
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
                    return xml(this);
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
                    return newType(this);
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
            //// TODO test the backwards compatibility of this with binary *and* source compatibility

            return metadataDto.Visit(
                known => known.Visit(
                    json, 
                    xml, 
                    newType => unknown(new MetadataDto.Unknown())),
                unknown);
        }

        public static TResult Apply<TResult>(
            this MetadataDto metadataDto,
            Func<MetadataDto.Known.Xml, TResult> xml,
            Func<MetadataDto.Known.Json, TResult> json,
            Func<MetadataDto.Known.NewType, TResult> newType,
            Func<MetadataDto.Unknown, TResult> unknown)
        {
            return metadataDto.Visit(
                known => known.Visit(
                    json,
                    xml,
                    newType),
                unknown);
        }
    }
}
