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
        /// <param name="wtrealm">The application identifier.</param>
        /// <param name="metadataAddress">The address to retrieve the wsFederation metadata from.</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseWsFederationAuthentication(this IAppBuilder app, string wtrealm, string metadataAddress)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (string.IsNullOrEmpty(wtrealm))
            {
                throw new ArgumentNullException("wtrealm");
            }
            if (string.IsNullOrEmpty(metadataAddress))
            {
                throw new ArgumentNullException("metadataAddress");
            }

            return app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions()
            {
                Wtrealm = wtrealm,
                MetadataAddress = metadataAddress,
            });
        }

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

            if (string.IsNullOrWhiteSpace(wsFederationOptions.TokenValidationParameters.ValidAudience))
            {
                wsFederationOptions.TokenValidationParameters.ValidAudience = wsFederationOptions.Wtrealm;
            }

            return app.Use<WsFederationAuthenticationMiddleware>(app, wsFederationOptions);
        }
    }
}