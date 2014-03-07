// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Owin
{
    using System;
    using Microsoft.Owin.Security.OpenIdConnect;

    /// <summary>
    /// Extension methods for using <see cref="OpenIdConnectAuthenticationMiddleware"/>
    /// </summary>
    /// 
    public static class OpenIdConnectAuthenticationExtensions
    {
        /// <summary>
        /// Adds the <see cref="OpenIdConnectAuthenticationMiddleware"/> into the OWIN runtime.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="openIdConnectOptions">A <see cref="OpenIdConnectAuthenticationOptions"/> contains settings for obtaining identities using the OpenIdConnect protocol.</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseOpenIdConnectAuthentication(this IAppBuilder app, OpenIdConnectAuthenticationOptions openIdConnectOptions)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            if (openIdConnectOptions == null)
            {
                throw new ArgumentNullException("openIdConnectOptions");
            }

            app.Use(typeof(OpenIdConnectAuthenticationMiddleware), app, openIdConnectOptions);
            return app;
        }
    }
}