using System.IO;

namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedItemBuilder
    {
        Stream Stream { get; }
    }
}