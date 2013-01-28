using System.IO;
using System.IO.Compression;

namespace Microsoft.Owin.Compression.Encoding
{
    public class DeflateEncoding : IEncoding
    {
        public string Name { get; set; }

        public Stream CompressTo(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Compress, true);
        }
    }
}