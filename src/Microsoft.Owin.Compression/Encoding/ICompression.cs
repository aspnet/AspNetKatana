using System.IO;

namespace Microsoft.Owin.Compression.Encoding
{
    public interface ICompression
    {
        string Name { get; }
        Stream CompressTo(Stream stream);
    }
}
