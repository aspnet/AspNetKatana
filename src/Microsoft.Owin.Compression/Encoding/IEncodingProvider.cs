namespace Microsoft.Owin.Compression.Encoding
{
    public interface IEncodingProvider
    {
        IEncoding GetCompression(string encoding);
    }
}