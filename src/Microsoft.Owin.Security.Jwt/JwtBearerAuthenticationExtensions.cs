// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;

namespace Owin
{
    /// <summary>
    /// Extension methods provided by the JWT bearer token middleware.
    /// </summary>
    public static class JwtBearerAuthenticationExtensions
    {
        /// <summary>
        /// Adds JWT bearer token middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method.</param>
        /// <param name="options">An options class that controls the middleware behavior.</param>
        /// <returns>The original app parameter.</returns>
        public static IAppBuilder UseJwtBearerAuthentication(this IAppBuilder app, JwtBearerAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = options.Realm,
                Provider = options.Provider,
                AccessTokenFormat = new JwtFormat(options.AllowedAudiences, options.IssuerSecurityTokenProviders, options.SigningCredentialsProvider),
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);

            return app;
        }

        /// <summary>
        /// Adds JWT bearer token middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method.</param>
        /// <param name="issuer">The issuer to use when creating JWTs.</param>
        /// <param name="options">An options class that controls the middleware behavior.</param>
        /// <returns>The original app parameter.</returns>
        public static IAppBuilder UseSelfSignedJwtBearerToken(this IAppBuilder app, string issuer, JwtBearerAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = options.Realm,
                Provider = options.Provider,
                AccessTokenFormat = new SelfSignedJwtFormat(issuer),
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);

            return app;
        }
    }
}
