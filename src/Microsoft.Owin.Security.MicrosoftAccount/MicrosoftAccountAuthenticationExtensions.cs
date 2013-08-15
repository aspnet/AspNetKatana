// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.MicrosoftAccount;

namespace Owin
{
    /// <summary>
    /// Extension methods for using <see cref="MicrosoftAccountAuthenticationMiddleware"/>
    /// </summary>
    public static class MicrosoftAccountAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using Microsoft Account
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseMicrosoftAccountAuthentication(this IAppBuilder app, MicrosoftAccountAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(MicrosoftAccountAuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Microsoft Account
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="clientId">The application client ID assigned by the Microsoft authentication service</param>
        /// <param name="clientSecret">The application client secret assigned by the Microsoft authentication service</param>
        /// <returns></returns>
        public static IAppBuilder UseMicrosoftAccountAuthentication(
            this IAppBuilder app,
            string clientId,
            string clientSecret)
        {
            return UseMicrosoftAccountAuthentication(
                app,
                new MicrosoftAccountAuthenticationOptions
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
                });
        }
    }
}
