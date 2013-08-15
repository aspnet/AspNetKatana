// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;

namespace Owin
{
    /// <summary>
    /// Extension methods provided by the Windows Azure Active Directory JWT bearer token middleware.
    /// </summary>
    public static class WindowsAzureActiveDirectoryBearerAuthenticationExtensions
    {
        private const string SecurityTokenServiceAddressFormat = "https://login.windows.net/{0}/federationmetadata/2007-06/federationmetadata.xml";

        /// <summary>
        /// Adds Windows Azure Active Directory (WAAD) issued JWT bearer token middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method.</param>
        /// <param name="options">An options class that controls the middleware behavior.</param>
        /// <returns>The original app parameter.</returns>
        public static IAppBuilder UseWindowsAzureActiveDirectoryBearerAuthentication(this IAppBuilder app, WindowsAzureActiveDirectoryBearerAuthenticationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = options.Realm,
                Provider = options.Provider,
                AccessTokenFormat = new JwtFormat(new[] { options.Audience }, new[]
                {
                    new WsFedCachingSecurityTokenProvider(
                        string.Format(CultureInfo.InvariantCulture, SecurityTokenServiceAddressFormat, options.Tenant),
                        true)
                }),
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);

            return app;
        }
    }
}
