using Playground.TopLayer.Odata;

namespace Playground.TopLayer.Graph
{
    public sealed class SchemaVersion
    {
        private SchemaVersion()
        {
        }
        
        // NOTE: graph only has a single schema version right now, we just needed a placeholder type
    }

    public interface ISource
    {
        IMetadataSource<SchemaVersion> Metadata();

        void Batch(); //// TODO
    }

    public interface IComplianceSource
    {
    }

    public interface IMeSource
    {
    }

    public interface IUsersSource
    {
    }

    public interface IApplicationsSource
    {
    }
}
