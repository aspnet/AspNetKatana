// <copyright file="GoogleAuthenticationExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
                new GoogleAuthenticationOptions
                {
                    SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
                });
        }
    }
}
