namespace Fx.Parsing.Reader
{
    using System.Threading.Tasks;

    public static class ContextExtensions
    {
        public static async ValueTask Read(this Context context)
        {
            context.ValidBytes = await context.Stream.ReadAsync(context.Buffer).ConfigureAwait(false);
            context.CurrentByteIndex = 0;
        }
    }
}
