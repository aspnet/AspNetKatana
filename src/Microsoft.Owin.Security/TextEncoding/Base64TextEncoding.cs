using System;

namespace Microsoft.Owin.Security.TextEncoding
{
    public class Base64TextEncoding : ITextEncoding
    {
        public string Encode(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public byte[] Decode(string text)
        {
            return Convert.FromBase64String(text);
        }
    }
}