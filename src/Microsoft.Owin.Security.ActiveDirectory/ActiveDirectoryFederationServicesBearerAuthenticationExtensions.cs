// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
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

            var cachingSecurityTokenProvider = new WsFedCachingSecurityTokenProvider(options.MetadataEndpoint,
                    options.BackchannelCertificateValidator, options.BackchannelTimeout, options.BackchannelHttpHandler);

            JwtFormat jwtFormat = null;
            if (options.TokenValidationParameters != null)
            {
                // Don't override explicit user settings.
                if (options.TokenValidationParameters.IssuerSigningTokens == null
                    || !options.TokenValidationParameters.IssuerSigningTokens.Any())
                {
                    options.TokenValidationParameters.IssuerSigningTokens = cachingSecurityTokenProvider.SecurityTokens;
                }
                if (string.IsNullOrWhiteSpace(options.TokenValidationParameters.ValidIssuer)
                    && (options.TokenValidationParameters.ValidIssuers == null
                        || !options.TokenValidationParameters.ValidIssuers.Any())
                    && options.TokenValidationParameters.IssuerValidator == null)
                {
                    options.TokenValidationParameters.ValidIssuer = cachingSecurityTokenProvider.Issuer;
                }
                // Carry over obsolete property if set
                if (!string.IsNullOrWhiteSpace(options.Audience))
                {
                    options.TokenValidationParameters.ValidAudience = options.Audience;
                }
                jwtFormat = new JwtFormat(options.TokenValidationParameters);
            }
            else
            {
                jwtFormat = new JwtFormat(options.Audience, cachingSecurityTokenProvider);
            }
            jwtFormat.TokenHandler = options.TokenHandler;

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
