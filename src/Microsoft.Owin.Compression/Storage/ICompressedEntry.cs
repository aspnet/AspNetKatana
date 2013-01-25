namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedEntry
    {
        string PhysicalPath { get; }
        long CompressedLength { get; }
    }
}