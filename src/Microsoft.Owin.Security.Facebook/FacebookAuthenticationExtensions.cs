// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Facebook;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="FacebookAuthenticationMiddleware"/>
    /// </summary>
    public static class FacebookAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using Facebook
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseFacebookAuthentication(this IAppBuilder app, FacebookAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(FacebookAuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Facebook
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="appId">The appId assigned by Facebook</param>
        /// <param name="appSecret">The appSecret assigned by Facebook</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseFacebookAuthentication(
            this IAppBuilder app,
            string appId,
            string appSecret)
        {
            return UseFacebookAuthentication(
                app,
                new FacebookAuthenticationOptions
                {
                    AppId = appId,
                    AppSecret = appSecret,
                });
        }
    }
}
