// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;

using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;

namespace Owin
{
    /// <summary>
    /// Extension methods provided by the ADFS JWT bearer token middleware.
    /// </summary>
    public static class ActiveDirectoryFederationServicesBearerAuthenticationExtensions
    {
        /// <summary>
        /// Adds Active Directory Federation Services (ADFS) issued JWT bearer token middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method.</param>
        /// <param name="options">An options class that controls the middleware behavior.</param>
        /// <returns>The original app parameter.</returns>
        public static IAppBuilder UseActiveDirectoryFederationServicesBearerAuthentication(this IAppBuilder app, ActiveDirectoryFederationServicesBearerAuthenticationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            JwtFormat jwtFormat = null;
            if (options.TokenValidationParameters != null)
            {
                jwtFormat = new JwtFormat(options.TokenValidationParameters);
            }
            else
            {
                jwtFormat = new JwtFormat(options.Audience, new WsFedCachingSecurityTokenProvider(options.MetadataEndpoint,
                    options.BackchannelCertificateValidator, options.BackchannelTimeout, options.BackchannelHttpHandler));
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = options.Realm,
                Provider = options.Provider,
                AccessTokenFormat = jwtFormat,
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);

            return app;
        }
    }
}
