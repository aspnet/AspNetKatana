using Microsoft.Owin.Compression.Encoding;
using Microsoft.Owin.Compression.Storage;

namespace Microsoft.Owin.Compression
{
    public class StaticCompressionOptions
    {
        public IEncodingProvider EncodingProvider { get; set; }
        public ICompressedStorageProvider CompressedStorageProvider { get; set; }
    }
}
