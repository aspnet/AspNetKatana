namespace Microsoft.Owin.Security.TextEncoding
{
    public interface ITextEncoding
    {
        string Encode(byte[] data);
        byte[] Decode(string text);
    }
}