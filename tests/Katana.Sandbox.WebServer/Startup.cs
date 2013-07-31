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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Katana.Sandbox.WebServer;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Katana.Sandbox.WebServer
{
    public class Startup
    {
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        public void Configuration(IAppBuilder app)
        {
            var logger = app.CreateLogger("Katana.Sandbox.WebServer");

            logger.WriteInformation("Application Started");

            app.Use(async (context, next) =>
            {
                context.Get<TextWriter>("host.TraceOutput").WriteLine("{0} {1}{2}", context.Request.Method, context.Request.PathBase, context.Request.Path);
                await next();
                context.Get<TextWriter>("host.TraceOutput").WriteLine("{0} {1}{2}", context.Response.StatusCode, context.Request.PathBase, context.Request.Path);
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Application",
                AuthenticationMode = AuthenticationMode.Passive,
                LoginPath = "/Login",
                LogoutPath = "/Logout",
            });

            app.SetDefaultSignInAsAuthenticationType("External");
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "External",
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = CookieAuthenticationDefaults.CookiePrefix + "External",
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
            });

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
            app.Use(async (context, next) =>
            {
                IOwinRequest req = context.Request;
                IOwinResponse res = context.Response;
                // for auth2 token requests, and web api requests
                if (req.Path == "/Token" || req.Path.StartsWith("/api/"))
                {
                    // if there is an origin header
                    var origin = req.Headers.Get("Origin");
                    if (!string.IsNullOrEmpty(origin))
                    {
                        // allow the cross-site request
                        res.Headers.Set("Access-Control-Allow-Origin", origin);
                    }

                    // if this is pre-flight request
                    if (req.Method == "OPTIONS")
                    {
                        // respond immediately with allowed request methods and headers
                        res.StatusCode = 200;
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Methods", "GET", "POST");
                        res.Headers.AppendCommaSeparatedValues("Access-Control-Allow-Headers", "authorization");
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
                AuthorizeEndpointDisplaysError = true,
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnValidateClientRedirectUri = ValidateClientRedirectUri,
                    OnValidateClientAuthentication = ValidateClientAuthentication,
                    OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials,
                },
                AuthorizationCodeProvider = new AuthenticationTokenProvider
                {
                    OnCreate = CreateAuthenticationCode,
                    OnReceive = ReceiveAuthenticationCode,
                },
                RefreshTokenProvider = new AuthenticationTokenProvider
                {
                    OnCreate = CreateRefreshToken,
                    OnReceive = ReceiveRefreshToken,
                }
            });

            app.Map("/api", map => map.Run(async context =>
            {
                var response = context.Response;
                var result = await context.Authentication.AuthenticateAsync("Bearer");
                if (result == null || result.Identity == null)
                {
                    context.Authentication.Challenge("Bearer");
                    return;
                }
                var identity = result.Identity;
                var properties = result.Properties.Dictionary;

                response.ContentType = "application/json";
                response.Write("{\"Details\":[");
                foreach (var claim in identity.Claims)
                {
                    response.Write("{\"Name\":\"");
                    response.Write(claim.Type);
                    response.Write("\",\"Value\":\"");
                    response.Write(claim.Value);
                    response.Write("\",\"Issuer\":\"");
                    response.Write(claim.Issuer);
                    response.Write("\"},"); // TODO: No comma on the last one
                }
                response.Write("],\"Properties\":[");
                foreach (var pair in properties)
                {
                    response.Write("{\"Name\":\"");
                    response.Write(pair.Key);
                    response.Write("\",\"Value\":\"");
                    response.Write(pair.Value);
                    response.Write("\"},"); // TODO: No comma on the last one
                }
                response.Write("]}");
            }));
        }

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == "123456")
            {
                context.Validated("http://localhost:18002/Katana.Sandbox.WebClient/ClientApp.aspx");
            }
            else if (context.ClientId == "7890ab")
            {
                context.Validated("http://localhost:18002/Katana.Sandbox.WebClient/ClientPageSignIn.html");
            }
            return Task.FromResult(0);
        }

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                if (clientId == "123456" && clientSecret == "abcdef")
                {
                    context.Validated();
                }
                else if (context.ClientId == "7890ab" && clientSecret == "7890ab")
                {
                    context.Validated();
                }
            }
            return Task.FromResult(0);
        }

        private Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(context.UserName, "Bearer"), context.Scope.Split(' ').Select(x => new Claim("urn:oauth:scope", x)));

            context.Validated(identity);

            return Task.FromResult(0);
        }

        private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
            }
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
        }
    }
}