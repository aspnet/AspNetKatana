using System.IO;

namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedEntryBuilder
    {
        Stream Stream { get; }
    }
}