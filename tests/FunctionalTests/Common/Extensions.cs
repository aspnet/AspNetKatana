// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace Owin
{
    internal static class CustomExtensions
    {
        #region Dictionary

        public static T Get<T>(this IDictionary<string, object> properties, string key)
        {
            return properties.ContainsKey(key) ? (T)properties[key] : default(T);
        }

        #endregion

        public static IAppBuilder UseAuthSignInCookie(this IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType("Application");

            return app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Application",
                AuthenticationMode = AuthenticationMode.Active
            });
        }

        public static void UseExternalApplication(this IAppBuilder app, string authenticationType)
        {
            app.Run(async context =>
                {
                    if (context.Authentication.User == null || !context.Authentication.User.Identity.IsAuthenticated)
                    {
                        context.Authentication.Challenge(authenticationType);
                        await context.Response.WriteAsync("Unauthorized");
                    }
                    else
                    {
                        var identity = context.Authentication.User.Identity as ClaimsIdentity;
                        if (identity.NameClaimType == "Name_Failed" && identity.RoleClaimType == "Role_Failed")
                        {
                            context.Response.StatusCode = 500;
                            await context.Response.WriteAsync("SignIn_Failed");
                        }
                        else if (!identity.HasClaim("Authenticated", "true") || !identity.HasClaim("ReturnEndpoint", "true") || !identity.HasClaim(identity.RoleClaimType, "Guest"))
                        {
                            await context.Response.WriteAsync("Provider not invoked");
                            return;
                        }
                        else
                        {
                            await context.Response.WriteAsync(authenticationType);
                        }
                    }
                });
        }

        public static void UseBearerApplication(this IAppBuilder app)
        {
            app.Run(async context =>
            {
                if (context.Authentication.User == null || !context.Authentication.User.Identity.IsAuthenticated)
                {
                    context.Authentication.Challenge("Bearer");
                    await context.Response.WriteAsync("Unauthorized");
                }
                else
                {
                    if (!context.Get<bool>("OnRequestToken") || !context.Get<bool>("OnValidateIdentity"))
                    {
                        await context.Response.WriteAsync("Provider not invoked");
                    }
                    else
                    {
                        await context.Response.WriteAsync("Bearer");
                    }
                }
            });
        }

        public static string GetWebConfigPath(this ApplicationDeployer applicationDeployer)
        {
            var webDeployer = (WebDeployer)applicationDeployer.Application;
            return Path.Combine(webDeployer.Application.VirtualDirectories[0].PhysicalPath, "web.config");
        }

        public static string GetFullyQualifiedConfigurationMethodName(this Action<IAppBuilder> configuration)
        {
            return configuration.Method.DeclaringType.FullName + "." + configuration.Method.Name;
        }

        public static string GetApplicationName(this Action<IAppBuilder> configuration)
        {
            return configuration.Method.Name.Replace('<', 'a').Replace('>', 'a');
        }

        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            return ParseItems(uri.Query);
        }

        public static async Task<Dictionary<string, string>> ReadAsFormDataAsync(this HttpContent httpContent)
        {
            return ParseItems(await httpContent.ReadAsStringAsync());
        }

        private static Dictionary<string, string> ParseItems(string content)
        {
            var items = new Dictionary<string, string>();
            if (content != null)
            {
                var contentString = Uri.UnescapeDataString(content.TrimStart('?'));

                var parts = contentString
                    .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(q => q.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries));

                foreach (var item in parts)
                {
                    var value = item.Length == 2 ? item[1] : null;
                    items.Add(item[0], value);
                }
            }

            return items;
        }
    }
}