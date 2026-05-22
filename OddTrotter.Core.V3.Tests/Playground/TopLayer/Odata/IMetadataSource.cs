namespace Playground.TopLayer.Odata
{
    using System.Threading.Tasks;

    //// TODO you are here
    //// TODO finish this file, including todos

    public interface IMetadataSource<TSchemaVersion>
    {
        IMetadataContext<TSchemaVersion> Get();

        //// TODO other verbs
    }

    public interface IMetadataContext<TSchemaVersion>
    {
        ITask<MetadataDto> Evaluate(); //// TODO can't just return the dto, need to have control information, headers, etc.

        //// TODO headers

        IMetadataContext<TSchemaVersion> SchemaVersion(TSchemaVersion schemaVersion); //// TODO lots of implications, read through this: https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html#sec_SystemQueryOptionschemaversion
    }

    public class MetadataDto
    {
        //// TODO
    }
}
