// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Cookies
{
    internal class CookieAuthenticationMiddleware : AuthenticationMiddleware<CookieAuthenticationOptions>
    {
        private readonly ILogger _logger;

        public CookieAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, CookieAuthenticationOptions options)
            : base(next, options)
        {
            if (Options.Provider == null)
            {
                Options.Provider = new CookieAuthenticationProvider();
            }
            if (String.IsNullOrEmpty(Options.CookieName))
            {
                Options.CookieName = CookieAuthenticationDefaults.CookiePrefix + Options.AuthenticationType;
            }

            _logger = app.CreateLogger<CookieAuthenticationMiddleware>();

            if (Options.TicketDataFormat == null)
            {
                IDataProtector dataProtector = app.CreateDataProtector(
                    typeof(CookieAuthenticationMiddleware).FullName,
                    Options.AuthenticationType, "v1");

                Options.TicketDataFormat = new TicketDataFormat(dataProtector);
            }
            if (Options.CookieManager == null)
            {
                Options.CookieManager = new ChunkingCookieManager();
            }
        }

        protected override AuthenticationHandler<CookieAuthenticationOptions> CreateHandler()
        {
            return new CookieAuthenticationHandler(_logger);
        }
    }
}
