namespace Microsoft.Owin.Compression.Storage
{
    public class DefaultCompressedStorageProvider : ICompressedStorageProvider
    {
        public ICompressedStorage Create()
        {
            var storage = new DefaultCompressedStorage();
            storage.Initialize();
            return storage;
        }
    }
}