// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

            var cachingSecurityTokenProvider = new WsFedCachingSecurityKeyProvider(options.MetadataEndpoint,
                    options.BackchannelCertificateValidator, options.BackchannelTimeout, options.BackchannelHttpHandler);

#pragma warning disable 618
            JwtFormat jwtFormat = null;
            if (options.TokenValidationParameters != null)
            {
                if (!string.IsNullOrWhiteSpace(options.Audience))
                {
                    // Carry over obsolete property if set
                    if (string.IsNullOrWhiteSpace(options.TokenValidationParameters.ValidAudience))
                    {
                        options.TokenValidationParameters.ValidAudience = options.Audience;
                    }
                    else if (options.TokenValidationParameters.ValidAudiences == null)
                    {
                        options.TokenValidationParameters.ValidAudiences = new[] { options.Audience };
                    }
                    else
                    {
                        options.TokenValidationParameters.ValidAudiences = options.TokenValidationParameters.ValidAudiences.Concat(new[] { options.Audience });
                    }
                }

                jwtFormat = new JwtFormat(options.TokenValidationParameters, cachingSecurityTokenProvider);
            }
            else
            {
                jwtFormat = new JwtFormat(options.Audience, cachingSecurityTokenProvider);
            }
#pragma warning restore 618
            if (options.TokenHandler != null)
            {
                jwtFormat.TokenHandler = options.TokenHandler;
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
