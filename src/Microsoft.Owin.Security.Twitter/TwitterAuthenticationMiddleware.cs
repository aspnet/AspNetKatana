// <copyright file="TwitterAuthenticationMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Twitter.Messages;

using Owin;

namespace Microsoft.Owin.Security.Twitter
{
    public class TwitterAuthenticationMiddleware : AuthenticationMiddleware<TwitterAuthenticationOptions>
    {
        private readonly SecureDataHandler<RequestToken> _stateHandler;
        private readonly ILogger _logger;

        public TwitterAuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            TwitterAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<TwitterAuthenticationMiddleware>();

            if (Options.Provider == null)
            {
                Options.Provider = new TwitterAuthenticationProvider();
            }

            IDataProtecter dataProtecter = Options.DataProtection;
            if (Options.DataProtection == null)
            {
                dataProtecter = app.CreateDataProtecter("TwitterAuthenticationMiddleware", Options.AuthenticationType);
            }

            _stateHandler = new SecureDataHandler<RequestToken>(
                Serializers.RequestToken,
                dataProtecter,
                TextEncodings.Base64Url);
        }

        protected override AuthenticationHandler<TwitterAuthenticationOptions> CreateHandler()
        {
            return new TwitterAuthenticationHandler(_logger, _stateHandler);
        }
    }
}
