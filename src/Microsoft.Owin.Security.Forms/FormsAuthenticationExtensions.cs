// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Forms;

namespace Owin
{
    public static class FormsAuthenticationExtensions
    {
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

        public static IAppBuilder UseFormsAuthentication(this IAppBuilder app, Action<FormsAuthenticationOptions> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var options = new FormsAuthenticationOptions();
            configuration(options);
            return UseFormsAuthentication(app, options);
        }

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
