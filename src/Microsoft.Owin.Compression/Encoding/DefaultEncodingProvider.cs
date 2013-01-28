using System;

namespace Microsoft.Owin.Compression.Encoding
{
    public class DefaultEncodingProvider : IEncodingProvider
    {
        public IEncoding GetCompression(string encoding)
        {
            if (string.Equals(encoding, "deflate", StringComparison.OrdinalIgnoreCase))
            {
                return new DeflateEncoding { Name = encoding };
            }
            if (string.Equals(encoding, "gzip", StringComparison.OrdinalIgnoreCase))
            {
                return new GZipEncoding { Name = encoding };
            }
            return null;
        }
    }
}