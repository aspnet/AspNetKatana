// <copyright file="FormsAuthenticationMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationMiddleware : AuthenticationMiddleware<FormsAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public FormsAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, FormsAuthenticationOptions options)
            : base(next, options)
        {
            if (options.Provider == null)
            {
                options.Provider = new FormsAuthenticationProvider();
            }

            _logger = app.CreateLogger<FormsAuthenticationMiddleware>();

            if (options.TicketDataHandler == null)
            {
                IDataProtector dataProtecter = app.CreateDataProtector(
                    typeof(FormsAuthenticationMiddleware).FullName,
                    Options.AuthenticationType);

                options.TicketDataHandler = new TicketDataHandler(dataProtecter);
            }
        }

        protected override AuthenticationHandler<FormsAuthenticationOptions> CreateHandler()
        {
            return new FormsAuthenticationHandler(_logger);
        }
    }
}
