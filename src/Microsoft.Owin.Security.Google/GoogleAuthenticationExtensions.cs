// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

// OpenID is obsolete
#pragma warning disable 618

using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Google;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="GoogleAuthenticationMiddleware"/>
    /// </summary>
    public static class GoogleAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using Google OpenId
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        [Obsolete("Google is discontinuing support for the OpenId. Use OAuth2 instead.", error: false)]
        public static IAppBuilder UseGoogleAuthentication(this IAppBuilder app, GoogleAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(GoogleAuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Google OpenId
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        [Obsolete("Google is discontinuing support for the OpenId. Use OAuth2 instead.", error: false)]
        public static IAppBuilder UseGoogleAuthentication(
            this IAppBuilder app)
        {
            return UseGoogleAuthentication(
                app,
                new GoogleAuthenticationOptions());
        }

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
#pragma warning restore 618