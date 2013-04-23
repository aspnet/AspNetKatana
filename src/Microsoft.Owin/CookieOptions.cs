using System;

namespace Microsoft.Owin
{
    public class CookieOptions
    {
        public CookieOptions()
        {
            Path = "/";
        }
        public string Domain { get; set; }
        public string Path { get; set; }
        public DateTime? Expires { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
    }
}