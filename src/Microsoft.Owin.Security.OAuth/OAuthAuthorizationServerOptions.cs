// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerOptions : AuthenticationOptions
    {
        public OAuthAuthorizationServerOptions() : base("Bearer")
        {
            AuthorizationCodeExpireTimeSpan = TimeSpan.FromMinutes(5);
            AccessTokenExpireTimeSpan = TimeSpan.FromDays(14);
            SystemClock = new SystemClock();
        }

        public string AuthorizeEndpointPath { get; set; }
        public string TokenEndpointPath { get; set; }

        public IOAuthAuthorizationServerProvider Provider { get; set; }

        public ISecureDataFormat<AuthenticationTicket> AuthorizationCodeFormat { get; set; }
        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; set; }
        public ISecureDataFormat<AuthenticationTicket> RefreshTokenFormat { get; set; }

        public TimeSpan AuthorizationCodeExpireTimeSpan { get; set; }
        public TimeSpan AccessTokenExpireTimeSpan { get; set; }

        public IAuthenticationTokenProvider AuthorizationCodeProvider { get; set; }
        public IAuthenticationTokenProvider AccessTokenProvider { get; set; }
        public IAuthenticationTokenProvider RefreshTokenProvider { get; set; }

        public bool AuthorizeEndpointDisplaysError { get; set; }

        public ISystemClock SystemClock { get; set; }
    }
}
