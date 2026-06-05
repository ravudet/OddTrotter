namespace Fx.Parsing.Reader
{
    using System.IO;
    using System.Threading.Tasks;

    public sealed class Context
    {
        private Context(
            Stream stream,
            byte[] buffer,
            int currentByteIndex,
            int validBytes)
        {
            Stream = stream;
            Buffer = buffer;
            CurrentByteIndex = currentByteIndex;
            ValidBytes = validBytes;
        }

        public Stream Stream { get; }
        public byte[] Buffer { get; }
        public int CurrentByteIndex { get; set; }
        public int ValidBytes { get; set; }

        public static async Task<Context> FromStream(Stream stream, byte[] buffer)
        {
            var context = new Context(stream, buffer, 0, 0);
            await context.Read().ConfigureAwait(false);
            return context;
        }
    }
}
