using System;

namespace Microsoft.Owin.Compression.Encoding
{
    public interface ICompressionProvider
    {
        ICompression GetCompression(string encoding);
    }

    public class DefaultCompressionProvider : ICompressionProvider
    {
        public ICompression GetCompression(string encoding)
        {
            if (string.Equals(encoding, "deflate", StringComparison.OrdinalIgnoreCase))
            {
                return new DeflateCompression { Name = encoding };
            }
            if (string.Equals(encoding, "gzip", StringComparison.OrdinalIgnoreCase))
            {
                return new GZipCompression { Name = encoding };
            }
            return null;
        }
    }
}