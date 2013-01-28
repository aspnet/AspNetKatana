namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedStorageProvider
    {
        ICompressedStorage Create();
    }
}