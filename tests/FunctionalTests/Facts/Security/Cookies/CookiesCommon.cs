// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Owin;
using Xunit;

namespace FunctionalTests.Facts.Security
{
    public static class CookiesCommon
    {
        public static void IsRedirectedToCookiesLogin(Uri requestUri, string protectedResourceUri, string message, string queryParameterName = "ReturnUrl")
        {
            Assert.True(requestUri.AbsolutePath.EndsWith("/Auth/CookiesLogin"), message);
            Assert.Equal<string>(new Uri(protectedResourceUri).AbsolutePath, requestUri.ParseQueryString()[queryParameterName]);
        }

        public static void UseCookiesLoginSetup(this IAppBuilder app)
        {
            app.Map("/Auth/CookiesLogin", login =>
            {
                login.Run(async context =>
                {
                    if (context.Request.Method == "POST")
                    {
                        var formData = await context.Request.ReadFormAsync();
                        if (formData.Get("username") == "test" && formData.Get("password") == "test")
                        {
                            var identity = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, formData.Get("username")) }, "Cookies", ClaimTypes.Name, ClaimTypes.Role);
                            var authProperties = new AuthenticationProperties() { IsPersistent = formData.Get("rememberme") == "on" };
                            context.Authentication.SignIn(authProperties, identity);
                        }
                        else
                        {
                            context.Response.StatusCode = 302;
                            context.Response.Headers.Add("Location", new string[] { context.Request.PathBase.Value + context.Request.Path.Value + context.Request.QueryString });
                        }
                    }
                    else
                    {
                        await context.Response.WriteAsync("<html><head><title>Login page</title></head><body><form method=\"post\">UserName: <input type=\"text\" name=\"userName\" /><br />Password: <input type=\"password\" name=\"password\" /><br /><br />Remember Me: <input type=\"checkbox\" name=\"rememberme\" /><br /><input type=\"submit\" /></form></body></html>");
                    }
                });
            });

            app.Map("/Auth/Logout", logout =>
            {
                logout.Run(async context =>
                {
                    context.Authentication.SignOut("Cookies", "Application");
                    context.Request.Path = context.Request.PathBase;
                    await context.Response.WriteAsync("Logout");
                });
            });

            app.Map("/Auth/Home", home =>
                {
                    home.Run(async context =>
                    {
                        await context.Response.WriteAsync("Welcome Home");
                    });
                });

            app.Map("/Auth/PassiveAuthLogin", passiveLogin =>
            {
                passiveLogin.Run(async context =>
                {
                    var ticket = await context.Authentication.AuthenticateAsync("Cookies");

                    if (ticket != null)
                    {
                        context.Authentication.SignIn(new ClaimsIdentity(ticket.Identity.Claims, "Application"));
                        context.Response.StatusCode = 302;
                        context.Response.Headers.Add("Location", new string[] { context.Request.PathBase.Value.Replace("/Auth/PassiveAuthLogin", string.Empty) + "/" });
                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                        context.Authentication.Challenge("Cookies");
                    }
                });
            });
        }

        public static void UseProtectedResource(this IAppBuilder app)
        {
            app.UseCookiesLoginSetup();

            app.Run(async context =>
                {
                    if (context.Authentication.User == null || !context.Authentication.User.Identity.IsAuthenticated)
                    {
                        context.Authentication.Challenge("Cookies");
                        await context.Response.WriteAsync("Unauthorized");
                    }
                    else
                    {
                        var identity = context.Request.User.Identity as ClaimsIdentity;
                        if (!identity.HasClaim("ResponseSignIn", "true") || !identity.HasClaim("ValidateIdentity", "true"))
                        {
                            throw new Exception("Forms AuthenticationProvider not invoked as expected");
                        }

                        await context.Response.WriteAsync("ProtectedResource");
                    }
                });
        }
    }
}