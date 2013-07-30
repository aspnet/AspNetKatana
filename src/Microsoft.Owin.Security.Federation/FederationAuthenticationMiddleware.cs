// <copyright file="FederationAuthenticationMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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

using System.IdentityModel.Services.Configuration;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Federation
{
    public class FederationAuthenticationMiddleware : AuthenticationMiddleware<FederationAuthenticationOptions>
    {
        private readonly FederationConfiguration _federationConfiguration;
        private readonly ILogger _logger;

        public FederationAuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            FederationAuthenticationOptions options) : base(next, options)
        {
            _logger = app.CreateLogger<FederationAuthenticationMiddleware>();

            _federationConfiguration = Options.FederationConfiguration ?? new FederationConfiguration(loadConfig: true);

            if (Options.StateDataFormat == null)
            {
                var dataProtector = app.CreateDataProtector(
                    typeof(FederationAuthenticationMiddleware).FullName,
                    Options.AuthenticationType, "v1");
                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }
        }

        protected override AuthenticationHandler<FederationAuthenticationOptions> CreateHandler()
        {
            return new FederationAuthenticationHandler(_logger, _federationConfiguration);
        }
    }
}
