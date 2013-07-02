// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Cookies
{
    internal class CookiesAuthenticationMiddleware : AuthenticationMiddleware<CookiesAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public CookiesAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, CookiesAuthenticationOptions options)
            : base(next, options)
        {
            if (Options.Provider == null)
            {
                Options.Provider = new CookiesAuthenticationProvider();
            }

            _logger = app.CreateLogger<CookiesAuthenticationMiddleware>();

            if (Options.TicketDataHandler == null)
            {
                IDataProtector dataProtector = app.CreateDataProtector(
                    typeof(CookiesAuthenticationMiddleware).FullName,
                    Options.AuthenticationType);

                Options.TicketDataHandler = new TicketDataHandler(dataProtector);
            }
        }

        protected override AuthenticationHandler<CookiesAuthenticationOptions> CreateHandler()
        {
            return new CookiesAuthenticationHandler(_logger);
        }
    }
}
