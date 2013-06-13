// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Forms
{
    internal class FormsAuthenticationMiddleware : AuthenticationMiddleware<FormsAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public FormsAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, FormsAuthenticationOptions options)
            : base(next, options)
        {
            if (Options.Provider == null)
            {
                Options.Provider = new FormsAuthenticationProvider();
            }

            _logger = app.CreateLogger<FormsAuthenticationMiddleware>();

            if (Options.TicketDataHandler == null)
            {
                IDataProtector dataProtector = app.CreateDataProtector(
                    typeof(FormsAuthenticationMiddleware).FullName,
                    Options.AuthenticationType);

                Options.TicketDataHandler = new TicketDataHandler(dataProtector);
            }
        }

        protected override AuthenticationHandler<FormsAuthenticationOptions> CreateHandler()
        {
            return new FormsAuthenticationHandler(_logger);
        }
    }
}
