// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        /// Authenticate users using Google
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
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
        /// Authenticate users using Google
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseGoogleAuthentication(
            this IAppBuilder app)
        {
            return UseGoogleAuthentication(
                app,
                new GoogleAuthenticationOptions());
        }
    }
}
