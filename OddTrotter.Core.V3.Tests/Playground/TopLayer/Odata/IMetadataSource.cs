namespace Playground.TopLayer.Odata
{
    using System.Threading.Tasks;

    public interface IMetadataSource<TSchemaVersion>
    {
        IMetadataContext<TSchemaVersion> Get();
    }

    public interface IMetadataContext<TSchemaVersion>
    {
        ITask<MetadataDto> Evaluate();

        IMetadataContext<TSchemaVersion> SchemaVersion(TSchemaVersion schemaVersion); //// TODO lots of implications, read through this: https://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html#sec_SystemQueryOptionschemaversion
    }

    public class MetadataDto
    {
        //// TODO
    }
}
