// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security.WsFederation;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="WsFederationAuthenticationMiddleware"/>
    /// </summary>
    public static class WsFederationAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="WsFederationAuthenticationMiddleware"/> into the OWIN runtime.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="wsFederationOptions">WsFederationAuthenticationOptions configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseWsFederationAuthentication(this IAppBuilder app, WsFederationAuthenticationOptions wsFederationOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (wsFederationOptions == null)
            {
                throw new ArgumentNullException("wsFederationOptions");
            }

            app.Use<WsFederationAuthenticationMiddleware>(app, wsFederationOptions);
            wsFederationOptions.TokenValidationParameters.ValidAudience = wsFederationOptions.Wtrealm;
            return app;
        }
    }
}