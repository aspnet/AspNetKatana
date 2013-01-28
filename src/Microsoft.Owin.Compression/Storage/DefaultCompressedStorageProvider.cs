namespace Microsoft.Owin.Compression.Storage
{
    public class DefaultCompressedStorageProvider : ICompressedStorageProvider
    {
        public ICompressedStorage Create()
        {
            var storage = new DefaultCompressedStorage();
            storage.Open();
            return storage;
        }
    }
}