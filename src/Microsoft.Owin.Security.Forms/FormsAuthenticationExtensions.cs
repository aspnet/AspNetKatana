// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Forms;

namespace Owin
{
    /// <summary>
    /// Extension methods provided by forms authentication middleware
    /// </summary>
    public static class FormsAuthenticationExtensions
    {
        /// <summary>
        /// Adds a cookie-based authentication middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method</param>
        /// <param name="options">An options class that controls the middleware behavior</param>
        /// <returns>The original app parameter</returns>
        public static IAppBuilder UseFormsAuthentication(this IAppBuilder app, FormsAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            app.Use(typeof(FormsAuthenticationMiddleware), app, options);
            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }

        /// <summary>
        /// Adds an instance of the cookie-based middleware pre-configured for the
        /// requirements of the "Application" AuthenticationType. The AuthenticationMode
        /// is active, so the current user of the incoming requests will be altered, and
        /// a 401 will redirect to the default "/Account/Login" path.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method</param>
        /// <returns>The original app parameter</returns>
        public static IAppBuilder UseApplicationSignInCookie(this IAppBuilder app)
        {
            return UseFormsAuthentication(app, new FormsAuthenticationOptions
            {
                AuthenticationType = FormsAuthenticationDefaults.ApplicationAuthenticationType,
                AuthenticationMode = AuthenticationMode.Active,
                CookieName = FormsAuthenticationDefaults.CookiePrefix + FormsAuthenticationDefaults.ApplicationAuthenticationType,
                LoginPath = FormsAuthenticationDefaults.LoginPath,
                LogoutPath = FormsAuthenticationDefaults.LogoutPath,
            });
        }

        /// <summary>
        /// Adds an instance of the cookie-based middleware pre-configured for the
        /// requirements of the "External" AuthenticationType. The AuthenticationMode
        /// is passive, so the identity does not apply automatically to any requests. A request
        /// handler must call authenticate specifically asking for the "External" authentication 
        /// type in order to access the ClaimsIdentity. The expiration of the cookie is shortened 
        /// to five minutes. 
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method</param>
        /// <returns>The original app parameter</returns>
        public static IAppBuilder UseExternalSignInCookie(this IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(FormsAuthenticationDefaults.ExternalAuthenticationType);

            return UseFormsAuthentication(app, new FormsAuthenticationOptions
            {
                AuthenticationType = FormsAuthenticationDefaults.ExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = FormsAuthenticationDefaults.CookiePrefix + FormsAuthenticationDefaults.ExternalAuthenticationType,
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
            });
        }
    }
}
