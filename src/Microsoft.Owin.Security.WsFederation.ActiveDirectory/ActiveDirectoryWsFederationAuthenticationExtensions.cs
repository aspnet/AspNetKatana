// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security.WsFederation;
using Microsoft.Owin.Security.WsFederation.ActiveDirectory;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="WsFederationAuthenticationMiddleware"/>
    /// </summary>
    public static class ActiveDirectoryWsFederationAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using ActiveDirectory and WsFedertaion
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseActiveDirectoryWsFederationAuthentication(this IAppBuilder app, ActiveDirectoryWsFederationAuthenticationOptions wsFederationOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            if (wsFederationOptions == null)
            {
                throw new ArgumentNullException("wsFederationOptions");
            }

            if (string.IsNullOrWhiteSpace(wsFederationOptions.Wtrealm))
            {
                throw new ArgumentException("wsFederationOptions.Wtrealm is null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(wsFederationOptions.Tenant))
            {
                throw new ArgumentException("wsFederationOptions.Tennant is null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(wsFederationOptions.IssuerAddress))
            {
                wsFederationOptions.IssuerAddress = ActiveDirectoryWsFederationEndpoints.IssuerAddress;
                if (!wsFederationOptions.Tenant.StartsWith("/"))
                {
                    wsFederationOptions.IssuerAddress += "/";
                }

                wsFederationOptions.IssuerAddress += wsFederationOptions.Tenant;
                if (!wsFederationOptions.Tenant.EndsWith("/"))
                {
                    wsFederationOptions.IssuerAddress += "/";
                }

                wsFederationOptions.MetadataAddress = wsFederationOptions.IssuerAddress + ActiveDirectoryWsFederationEndpoints.WsFedMetadata;
                wsFederationOptions.IssuerAddress += ActiveDirectoryWsFederationEndpoints.WsFed;
            }

            wsFederationOptions.TokenValidationParameters.ValidAudience = wsFederationOptions.Wtrealm;
            wsFederationOptions.Notifications = new WsFederationAuthenticationNotifications();           
            app.Use<WsFederationAuthenticationMiddleware>(app, wsFederationOptions);
            return app;
        }
    }
}
