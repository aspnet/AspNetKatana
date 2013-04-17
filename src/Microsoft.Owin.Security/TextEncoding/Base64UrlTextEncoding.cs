using System;

namespace Microsoft.Owin.Security.TextEncoding
{
    public class Base64UrlTextEncoding : ITextEncoding
    {
        public string Encode(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_');
        }

        public byte[] Decode(string text)
        {
            return Convert.FromBase64String(text.Replace('-', '+').Replace('_', '/'));
        }
    }
}