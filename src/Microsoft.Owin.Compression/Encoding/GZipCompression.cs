using System.IO;
using System.IO.Compression;

namespace Microsoft.Owin.Compression.Encoding
{
    public class GZipCompression : ICompression
    {
        public string Name { get; set; }

        public Stream CompressTo(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress);
        }
    }
}