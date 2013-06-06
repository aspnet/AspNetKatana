// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationOptions : AuthenticationOptions
    {
        public FormsAuthenticationOptions()
            : base(FormsAuthenticationDefaults.AuthenticationType)
        {
            CookieName = FormsAuthenticationDefaults.CookiePrefix + FormsAuthenticationDefaults.AuthenticationType;
            CookiePath = "/";
            ExpireTimeSpan = TimeSpan.FromDays(14);
            SlidingExpiration = true;
            CookieHttpOnly = true;
            CookieSecure = CookieSecureOption.SameAsRequest;
            SystemClock = new SystemClock();
        }

        public string CookieName { get; set; }
        public string CookieDomain { get; set; }
        public string CookiePath { get; set; }
        public bool CookieHttpOnly { get; set; }
        public CookieSecureOption CookieSecure { get; set; }

        public TimeSpan ExpireTimeSpan { get; set; }
        public bool SlidingExpiration { get; set; }

        public string LoginPath { get; set; }
        public string LogoutPath { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string ReturnUrlParameter { get; set; }

        public IFormsAuthenticationProvider Provider { get; set; }

        public ISecureDataHandler<AuthenticationTicket> TicketDataHandler { get; set; }

        public ISystemClock SystemClock { get; set; }
    }
}
