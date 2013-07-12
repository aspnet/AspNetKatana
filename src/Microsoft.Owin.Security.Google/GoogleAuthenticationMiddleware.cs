// <copyright file="GoogleAuthenticationMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Google
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Middleware are not disposable.")]
    public class GoogleAuthenticationMiddleware : AuthenticationMiddleware<GoogleAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public GoogleAuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            GoogleAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<GoogleAuthenticationMiddleware>();

            if (Options.Provider == null)
            {
                Options.Provider = new GoogleAuthenticationProvider();
            }
            if (Options.StateDataFormat == null)
            {
                var dataProtecter = app.CreateDataProtector(
                    typeof(GoogleAuthenticationMiddleware).FullName,
                    Options.AuthenticationType);
                Options.StateDataFormat = new ExtraDataFormat(dataProtecter);
            }

            _httpClient = new HttpClient(Options.HttpHandler);
            _httpClient.Timeout = Options.BackchannelTimeout;
        }

        protected override AuthenticationHandler<GoogleAuthenticationOptions> CreateHandler()
        {
            return new GoogleAuthenticationHandler(_httpClient, _logger);
        }
    }
}
