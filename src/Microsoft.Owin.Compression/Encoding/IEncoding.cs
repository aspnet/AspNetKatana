using System.IO;

namespace Microsoft.Owin.Compression.Encoding
{
    public interface IEncoding
    {
        string Name { get; }
        Stream CompressTo(Stream stream);
    }
}
