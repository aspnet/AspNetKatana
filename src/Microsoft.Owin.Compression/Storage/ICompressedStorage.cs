using System;

namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedStorage : IDisposable
    {
        ICompressedItemHandle Open(CompressedKey key);
        ICompressedItemBuilder Create(CompressedKey key);
        ICompressedItemHandle Commit(ICompressedItemBuilder builder);
    }
}
