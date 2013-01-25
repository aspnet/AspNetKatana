using Microsoft.Owin.Compression.Encoding;
using Microsoft.Owin.Compression.Storage;

namespace Microsoft.Owin.Compression
{
    public class StaticCompressionOptions
    {
        public ICompressionProvider CompressionProvider { get; set; }
        public ICompressedStorage CompressedStorage { get; set; }
    }
}
