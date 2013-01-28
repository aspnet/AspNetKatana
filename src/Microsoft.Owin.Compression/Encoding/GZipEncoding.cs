using System.IO;
using System.IO.Compression;

namespace Microsoft.Owin.Compression.Encoding
{
    public class GZipEncoding : IEncoding
    {
        public string Name { get; set; }

        public Stream CompressTo(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress, true);
        }
    }
}