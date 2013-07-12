// <copyright file="TwitterAuthenticationExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Security.Twitter;

namespace Owin
{
    public static class TwitterAuthenticationExtensions
    {
        public static IAppBuilder UseTwitterAuthentication(this IAppBuilder app, TwitterAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            ResolveHttpMessageHandler(options);
            app.Use(typeof(TwitterAuthenticationMiddleware), app, options);
            return app;
        }

        public static IAppBuilder UseTwitterAuthentication(
            this IAppBuilder app,
            string consumerKey,
            string consumerSecret)
        {
            return UseTwitterAuthentication(
                app,
                new TwitterAuthenticationOptions
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType(),
                });
        }

        private static void ResolveHttpMessageHandler(TwitterAuthenticationOptions options)
        {
            options.HttpHandler = options.HttpHandler ?? new WebRequestHandler();

            // Set the cert validate callback
            WebRequestHandler webRequestHandler = options.HttpHandler as WebRequestHandler;
            if (options.CertificateValidator != null && webRequestHandler == null)
            {
                throw new InvalidOperationException(Resources.Exception_ValidatorHandlerMismatch);
            }
            if (webRequestHandler.ServerCertificateValidationCallback == null)
            {
                if (options.CertificateValidator == null)
                {
                    // Twitter lists its valid Subject Key Identifiers at https://dev.twitter.com/docs/security/using-ssl
                    webRequestHandler.ServerCertificateValidationCallback = new CertificateSubjectKeyIdentifierValidator(
                        new[]
                        {
                            "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
                            "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
                        }).Validate;
                }
                else
                {
                    webRequestHandler.ServerCertificateValidationCallback = options.CertificateValidator.Validate;
                }
            }
        }
    }
}
