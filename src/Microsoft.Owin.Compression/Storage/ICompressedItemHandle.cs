using System;

namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedItemHandle : IDisposable
    {
        string PhysicalPath { get; }
        long CompressedLength { get; }
    }
}
