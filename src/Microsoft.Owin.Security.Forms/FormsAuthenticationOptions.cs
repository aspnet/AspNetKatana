using System;
using Microsoft.Owin.Security.DataProtection;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationOptions : AuthenticationOptions
    {
        public FormsAuthenticationOptions() : base("Forms")
        {
            CookieName = "ASPNETLOGIN";
            CookiePath = "/";
            ExpireTimeSpan = TimeSpan.FromMinutes(20);
        }

        public string CookieName { get; set; }
        public string CookieDomain { get; set; }
        public string CookiePath { get; set; }
        public bool CookieHttpOnly { get; set; }
        public bool CookieSecure { get; set; }

        public TimeSpan ExpireTimeSpan { get; set; }

        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }

        public IFormsAuthenticationProvider Provider { get; set; }

        public IDataProtection DataProtection { get; set; }
    }
}
