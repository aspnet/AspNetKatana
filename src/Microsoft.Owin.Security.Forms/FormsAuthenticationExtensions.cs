// <copyright file="FormsAuthenticationExtensions.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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
