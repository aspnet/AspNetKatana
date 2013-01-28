namespace Microsoft.Owin.Compression.Storage
{
    public interface ICompressedStorage
    {
        void Close();

        ICompressedEntry Lookup(CompressedKey key);

        ICompressedEntryBuilder Start(CompressedKey key);
        ICompressedEntry Finish(ICompressedEntryBuilder builder);
        void Abort(ICompressedEntryBuilder builder);
    }
}