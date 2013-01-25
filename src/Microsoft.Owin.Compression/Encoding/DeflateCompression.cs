using System.IO;
using System.IO.Compression;

namespace Microsoft.Owin.Compression.Encoding
{
    public class DeflateCompression : ICompression
    {
        public string Name { get; set; }

        public Stream CompressTo(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Compress);
        }
    }
}