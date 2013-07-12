// <copyright file="FacebookAuthenticationExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net.Http;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Facebook;

namespace Owin
{
    public static class FacebookAuthenticationExtensions
    {
        public static IAppBuilder UseFacebookAuthentication(this IAppBuilder app, FacebookAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            ResolveHttpMessageHandler(options);
            app.Use(typeof(FacebookAuthenticationMiddleware), app, options);
            return app;
        }

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
                    SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
                });
        }

        private static void ResolveHttpMessageHandler(FacebookAuthenticationOptions options)
        {
            options.HttpHandler = options.HttpHandler ?? new WebRequestHandler();

            // If they provided a validator, apply it or fail.
            if (options.CertificateValidator != null)
            {
                // Set the cert validate callback
                WebRequestHandler webRequestHandler = options.HttpHandler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
                }
                webRequestHandler.ServerCertificateValidationCallback = options.CertificateValidator.Validate;
            }
        }
    }
}
