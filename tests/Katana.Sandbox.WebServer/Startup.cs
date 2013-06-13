// <copyright file="Startup.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Forms;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof(Katana.Sandbox.WebServer.Startup))]

namespace Katana.Sandbox.WebServer
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var logger = app.CreateLogger("Katana.Sandbox.WebServer");

            logger.WriteInformation("Application Started");

            app.UseHandlerAsync(async (req, res, next) =>
            {
                req.TraceOutput.WriteLine("{0} {1}{2}", req.Method, req.PathBase, req.Path);
                await next();
                req.TraceOutput.WriteLine("{0} {1}{2}", res.StatusCode, req.PathBase, req.Path);
            });

            app.UseFormsAuthentication(new FormsAuthenticationOptions
            {
                AuthenticationType = "Application",
                AuthenticationMode = AuthenticationMode.Passive,
                LoginPath = "/Login",
                LogoutPath = "/Logout",
            });

            app.UseExternalSignInCookie();

            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                SignInAsAuthenticationType = "External",
                AppId = "615948391767418",
                AppSecret = "c9b1fa6b68db835890ce469e0d98157f",
                // Scope = "email user_birthday user_website"
            });

            app.UseGoogleAuthentication();

            app.UseTwitterAuthentication("6XaCTaLbMqfj6ww3zvZ5g", "Il2eFzGIrYhz6BWjYhVXBPQSfZuS4xoHpSSyD9PI");

            app.UseMicrosoftAccountAuthentication("000000004C0EA787", "QZde5m5HHZPxdieV0lOy7bBVTbVqR9Ju");

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
            });

            // CORS support
            app.UseHandlerAsync(async (req, res, next) =>
            {
                // for auth2 token requests, and web api requests
                if (req.Path == "/Token" || req.Path.StartsWith("/api/"))
                {
                    // if there is an origin header
                    var origin = req.GetHeader("Origin");
                    if (!string.IsNullOrEmpty(origin))
                    {
                        // allow the cross-site request
                        res.AddHeader("Access-Control-Allow-Origin", origin);
                    }

                    // if this is pre-flight request
                    if (req.Method == "OPTIONS")
                    {
                        // respond immediately with allowed request methods and headers
                        res.StatusCode = 200;
                        res.AddHeaderJoined("Access-Control-Allow-Methods", "GET", "POST");
                        res.AddHeaderJoined("Access-Control-Allow-Headers", "authorization");
                        // no further processing
                        return;
                    }
                }
                // continue executing pipeline
                await next();
            });

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/Authorize",
                TokenEndpointPath = "/Token",
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnValidateClientCredentials = OnValidateClientCredentials,
                    OnValidateResourceOwnerCredentials = OnValidateResourceOwnerCredentials,
                },
            });

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "api/{controller}");
            app.UseWebApi(config);

            app.UseDiagnosticsPage();
        }

        private Task OnValidateResourceOwnerCredentials(OAuthValidateResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(context.UserName, "Bearer"), context.Scope.Split(' ').Select(x => new Claim("urn:oauth:scope", x)));

            context.Validated(identity, null);

            return Task.FromResult<object>(null);
        }

        private Task OnValidateClientCredentials(OAuthValidateClientCredentialsContext context)
        {
            if (context.ClientId == "123456")
            {
                context.ClientFound("abcdef", "http://localhost:18002/Katana.Sandbox.WebClient/ClientApp.aspx");
            }
            else if (context.ClientId == "7890ab")
            {
                context.ClientFound("7890ab", "http://localhost:18002/Katana.Sandbox.WebClient/ClientPageSignIn.html");
            }
            return Task.FromResult<object>(null);
        }

        private class ConversionMiddleware : OwinMiddleware
        {
            private readonly AppFunc _appFunc;

            public ConversionMiddleware(AppFunc appFunc)
                : base(null)
            {
                _appFunc = appFunc;
            }
            public override Task Invoke(OwinRequest request, OwinResponse response)
            {
                return _appFunc.Invoke(response.Environment);
            }
        }
    }
}