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

#if AUTHSERVER

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerMiddleware : AuthenticationMiddleware<OAuthAuthorizationServerOptions>
    {
        private readonly ILogger _logger;

        public OAuthAuthorizationServerMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            OAuthAuthorizationServerOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<OAuthAuthorizationServerMiddleware>();

            if (Options.Provider == null)
            {
                Options.Provider = new OAuthAuthorizationServerProvider();
            }
            if (Options.AccessCodeHandler == null)
            {
                var dataProtecter = app.CreateDataProtector(
                    typeof(OAuthAuthorizationServerMiddleware).FullName, 
                    "Access Code");

                Options.AccessCodeHandler = new TicketDataHandler(dataProtecter);
            }
            if (Options.AccessTokenHandler == null)
            {
                var dataProtecter = app.CreateDataProtector(
                    typeof(OAuthAuthorizationServerMiddleware).Namespace, 
                    "Access Token");
                Options.AccessTokenHandler = new TicketDataHandler(dataProtecter);
            }
        }

        protected override AuthenticationHandler<OAuthAuthorizationServerOptions> CreateHandler()
        {
            return new OAuthAuthorizationServerHandler(_logger);
        }
    }
}

#endif
