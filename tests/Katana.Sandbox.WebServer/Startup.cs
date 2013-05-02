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
using Microsoft.Owin.Security.Forms;
using Microsoft.Owin.Security.OAuth;
using Owin;

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

            app.UseFacebookAuthentication("615948391767418", "c9b1fa6b68db835890ce469e0d98157f");

            app.UseGoogleAuthentication();

            app.UseTwitterAuthentication("6XaCTaLbMqfj6ww3zvZ5g", "Il2eFzGIrYhz6BWjYhVXBPQSfZuS4xoHpSSyD9PI");

            app.UseMicrosoftAccountAuthentication("6XaCTaLbMqfj6ww3zvZ5g", "Il2eFzGIrYhz6BWjYhVXBPQSfZuS4xoHpSSyD9PI");

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
            });

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/Authorize",
                TokenEndpointPath = "/Token",
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnValidateClientCredentials = OnValidateClientCredentials,
                    OnValidateResourceOwnerCredentials = OnValidateResourceOwnerCredentials
                },
            });

            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "api/{controller}");
            app.UseWebApi(config);
        }

        private async Task OnValidateResourceOwnerCredentials(OAuthValidateResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(context.Username, "Bearer"), context.Scope.Split(' ').Select(x => new Claim("urn:oauth:scope", x)));

            context.Validated(identity, null);
        }

        private async Task OnValidateClientCredentials(OAuthValidateClientCredentialsContext context)
        {
            if (context.ClientId == "123456")
            {
                context.ClientFound("abcdef", "http://localhost:18002/Katana.Sandbox.WebClient/ClientApp.aspx");
            }
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