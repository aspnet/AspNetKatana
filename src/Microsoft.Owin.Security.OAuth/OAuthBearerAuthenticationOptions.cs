// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthBearerAuthenticationOptions : AuthenticationOptions
    {
        public OAuthBearerAuthenticationOptions()
            : base("Bearer")
        {
            SystemClock = new SystemClock();
        }

        public string Realm { get; set; }

        public IOAuthBearerAuthenticationProvider Provider { get; set; }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; set; }

        public IAuthenticationTokenProvider AccessTokenProvider { get; set; }

        public ISystemClock SystemClock { get; set; }
    }
}
