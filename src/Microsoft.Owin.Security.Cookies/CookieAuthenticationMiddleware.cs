// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Cookie based authentication middleware
    /// </summary>
    public class CookieAuthenticationMiddleware : AuthenticationMiddleware<CookieAuthenticationOptions>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a <see cref="CookieAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
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

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="CookieAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<CookieAuthenticationOptions> CreateHandler()
        {
            return new CookieAuthenticationHandler(_logger);
        }
    }
}
