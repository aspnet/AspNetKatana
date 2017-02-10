// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security.Google;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="GoogleOAuth2AuthenticationMiddleware"/>
    /// </summary>
    public static class GoogleAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using Google OAuth 2.0
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
            Justification = "OAuth2 is a valid word.")]
        public static IAppBuilder UseGoogleAuthentication(this IAppBuilder app, GoogleOAuth2AuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(GoogleOAuth2AuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Google OAuth 2.0
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="clientId">The google assigned client id</param>
        /// <param name="clientSecret">The google assigned client secret</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
            Justification = "OAuth2 is a valid word.")]
        public static IAppBuilder UseGoogleAuthentication(
            this IAppBuilder app,
            string clientId,
            string clientSecret)
        {
            return UseGoogleAuthentication(
                app,
                new GoogleOAuth2AuthenticationOptions 
                { 
                    ClientId = clientId,
                    ClientSecret = clientSecret
                });
        }
    }
}