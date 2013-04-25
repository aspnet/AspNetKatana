// <copyright file="OAuthAuthorizationServerMiddleware.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using Microsoft.Owin.Security.DataSerializer;
using Microsoft.Owin.Security.TextEncoding;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerMiddleware : AuthenticationMiddleware<OAuthAuthorizationServerOptions>
    {
        private readonly ISecureDataHandler<AuthenticationTicket> _accessCodeHandler;
        private readonly ISecureDataHandler<AuthenticationTicket> _accessTokenHandler;

        public OAuthAuthorizationServerMiddleware(
            OwinMiddleware next,
            OAuthAuthorizationServerOptions options)
            : base(next, options)
        {
            // TODO - use different purposes - take these as options instead of the dataprotecter
            _accessCodeHandler = new SecureDataHandler<AuthenticationTicket>(
                DataSerializers.Ticket,
                Options.DataProtection,
                TextEncodings.Base64Url);
            _accessTokenHandler = new SecureDataHandler<AuthenticationTicket>(
                DataSerializers.Ticket,
                Options.DataProtection,
                TextEncodings.Base64Url);
        }

        protected override AuthenticationHandler<OAuthAuthorizationServerOptions> CreateHandler()
        {
            return new OAuthAuthorizationServerHandler(_accessCodeHandler, _accessTokenHandler);
        }
    }
}
