namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedStorage
    {
        ICompressedEntry Lookup(CompressedKey key);

        ICompressedEntryBuilder Start(CompressedKey key);
        ICompressedEntry Finish(CompressedKey key, ICompressedEntryBuilder builder);
        void Abort(ICompressedEntryBuilder builder);
    }
}