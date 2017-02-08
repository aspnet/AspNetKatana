// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Katana.Sandbox.WebServer;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.WsFederation;
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

            app.UseErrorPage(Microsoft.Owin.Diagnostics.ErrorPageOptions.ShowAll);

            app.Use(async (context, next) =>
            {
                context.Get<TextWriter>("host.TraceOutput").WriteLine("{0} {1}{2}", context.Request.Method, context.Request.PathBase, context.Request.Path);
                await next();
                context.Get<TextWriter>("host.TraceOutput").WriteLine("{0} {1}{2}", context.Response.StatusCode, context.Request.PathBase, context.Request.Path);
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Application",
                // LoginPath = new PathString("/Account/Login"),
            });

            app.SetDefaultSignInAsAuthenticationType("External");

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "External",
                AuthenticationMode = AuthenticationMode.Active,
                CookieName = CookieAuthenticationDefaults.CookiePrefix + "External",
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                CookieManager = new SystemWebChunkingCookieManager()
            });

            // https://developers.facebook.com/apps/
            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                AppId = Environment.GetEnvironmentVariable("facebook:appid"),
                AppSecret = Environment.GetEnvironmentVariable("facebook:appsecret"),
                Scope = { "email" },
                Fields = { "name", "email" },
                CookieManager = new SystemWebCookieManager()
            });

            // https://console.developers.google.com/project
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = Environment.GetEnvironmentVariable("google:clientid"),
                ClientSecret = Environment.GetEnvironmentVariable("google:clientsecret"),
            });

            //// Flow to get user identifier in OpenID for migration to OAuth 2.0
            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "448955186993-2vmtajdpl41ipktlg809780c0craq88e.apps.googleusercontent.com",
            //    ClientSecret = "Ngdb_GRmO3X2pC3WVt73Rod0",
            //    Provider = new GoogleOAuth2AuthenticationProvider()
            //    {
            //        OnApplyRedirect = context =>
            //        {
            //            // Add openid realm to get openid identifier in token response
            //            context.Response.Redirect(context.RedirectUri + "&openid.realm=http://localhost:49222/");
            //        },

            //        OnAuthenticated = context =>
            //        {
            //            var idToken = context.TokenResponse.Value<string>("id_token");
            //            var jwtIdToken = new JwtSecurityToken(idToken);
            //            var claims = jwtIdToken.Claims;

            //            var openid_id = claims.FirstOrDefault(x => x.Type.Equals("openid_id", StringComparison.CurrentCulture));
            //            return Task.FromResult(0);
            //        }
            //    }
            //});

            // https://apps.twitter.com/
            // https://dev.twitter.com/web/sign-in/implementing
            app.UseTwitterAuthentication(Environment.GetEnvironmentVariable("twitter:consumerkey"), Environment.GetEnvironmentVariable("twitter:consumersecret"));

            // https://azure.microsoft.com/en-us/documentation/articles/active-directory-v2-app-registration/
            app.UseMicrosoftAccountAuthentication(Environment.GetEnvironmentVariable("microsoftaccount:clientid"), Environment.GetEnvironmentVariable("microsoftaccount:clientsecret"));

            // app.UseAspNetAuthSession();
            /*
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                SessionStore = new InMemoryAuthSessionStore()
            });
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            */
            /*
            app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions()
            {
                Wtrealm = "http://Katana.Sandbox.WebServer",
                MetadataAddress = "https://login.windows.net/cdc690f9-b6b8-4023-813a-bae7143d1f87/FederationMetadata/2007-06/FederationMetadata.xml",
            });
            */

            app.UseOpenIdConnectAuthentication(new Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions()
            {
                Authority = Environment.GetEnvironmentVariable("oidc:authority"),
                ClientId = Environment.GetEnvironmentVariable("oidc:clientid"),
                RedirectUri = "https://localhost:44318/",
                CookieManager = new SystemWebCookieManager()
            });

            /*
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
            });

            // CORS support
            app.Use(async (context, next) =>
            {
                IOwinRequest req = context.Request;
                IOwinResponse res = context.Response;
                // for auth2 token requests, and web api requests
                if (req.Path.StartsWithSegments(new PathString("/Token")) ||
                    req.Path.StartsWithSegments(new PathString("/api")))
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
                AuthorizeEndpointPath = new PathString("/Authorize"),
                TokenEndpointPath = new PathString("/Token"),
                ApplicationCanDisplayErrors = true,
#if DEBUG
                AllowInsecureHttp = true,
#endif
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
            */
            app.Map("/signout", map =>
            {
                map.Run(context =>
                {
                    context.Authentication.SignOut("External");
                    var response = context.Response;
                    response.ContentType = "text/html";
                    response.Write("<body><html>Signed out. <a href=\"/\">Home</a></html></body>");
                    return Task.FromResult(0);
                });
            });
            app.Map("/challenge", map =>
            {
                map.Run(context =>
                {
                    context.Authentication.Challenge(new AuthenticationProperties() { RedirectUri = "/" }, context.Request.Query["scheme"]);
                    return Task.FromResult(0);
                });
            });
            /*
            app.Map("/Account/Login", map =>
            {
                map.Run(context =>
                {
                    // context.Authentication.Challenge("Google");
                    return Task.FromResult(0);
                });
            });
            */
            app.Use((context, next) =>
            {
                var user = context.Authentication.User;
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    var response = context.Response;
                    response.ContentType = "text/html";
                    response.Write("<html><body>Providers:<br>\r\n");
                    foreach (var provider in context.Authentication.GetAuthenticationTypes())
                    {
                        response.Write("- <a href=\"/challenge?scheme=");
                        response.Write(provider.AuthenticationType);
                        response.Write("\">");
                        response.Write(provider.AuthenticationType);
                        response.Write("</a><br>\r\n");
                    }
                    response.Write("</body></html>\r\n");
                    return Task.FromResult(0);
                }
                return next();
            });
            /*
            app.Use((context, next) =>
            {
                var user = context.Authentication.User;
                if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                {
                    context.Authentication.Challenge();
                    // context.Authentication.Challenge("Facebook");
                    return Task.FromResult(0);
                }
                return next();
            });
            */
            app.Run(async context =>
            {
                var response = context.Response;
                var user = context.Authentication.User;
                var identity = user.Identities.First();

                response.ContentType = "text/html";
                await response.WriteAsync("<html><body>Details:<br>\r\n");
                foreach (var claim in identity.Claims)
                {
                    response.Write("- ");
                    response.Write(claim.Type);
                    response.Write(": ");
                    response.Write(claim.Value);
                    response.Write("<br>\r\n");
                }
                response.Write("<a href=\"/signout\">Signout</a>\r\n");
                response.Write("</body></html>\r\n");
            });
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
            var identity = new ClaimsIdentity(new GenericIdentity(context.UserName, OAuthDefaults.AuthenticationType), context.Scope.Select(x => new Claim("urn:oauth:scope", x)));

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