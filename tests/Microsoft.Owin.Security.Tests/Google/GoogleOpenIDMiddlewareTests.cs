// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

// OpenID is obsolete
#pragma warning disable 618

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests.Google
{
    public class GoogleOpenIDMiddlewareTests
    {
        [Fact]
        public async Task ChallengeWillTriggerApplyRedirectEvent()
        {
            var options = new GoogleAuthenticationOptions()
            {
                Provider = new GoogleAuthenticationProvider
                {
                    OnApplyRedirect = context =>
                    {
                        context.Response.Redirect(context.RedirectUri + "&custom=test");
                    }
                }
            };
            var server = CreateServer(
                app => app.UseGoogleAuthentication(options),
                context =>
                {
                    context.Authentication.Challenge("Google");
                    return true;
                });
            var transaction = await SendAsync(server, "http://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            var query = transaction.Response.Headers.Location.Query;
            query.ShouldContain("custom=test");
        }

        [Fact]
        public async Task ChallengeWillTriggerRedirection()
        {
            var server = CreateServer(
                app => app.UseGoogleAuthentication(),
                context =>
                {
                    context.Authentication.Challenge("Google");
                    return true;
                });
            var transaction = await SendAsync(server, "http://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            var location = transaction.Response.Headers.Location.AbsoluteUri;
            location.ShouldContain("https://www.google.com/accounts/o8/ud");
            location.ShouldContain("?openid.ns=");
            location.ShouldContain("&openid.ns.ax=");
            location.ShouldContain("&openid.mode=");
            location.ShouldContain("&openid.claimed_id=");
            location.ShouldContain("&openid.identity=");
            location.ShouldContain("&openid.return_to=");
            location.ShouldContain("&openid.realm=");
            location.ShouldContain("&openid.ax.mode=");
        }

        private static TestServer CreateServer(Action<IAppBuilder> configure, Func<IOwinContext, bool> handler)
        {
            return TestServer.Create(app =>
            {
                app.Properties["host.AppName"] = "Microsoft.Owin.Security.Tests";
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "External"
                });
                app.SetDefaultSignInAsAuthenticationType("External");
                if (configure != null)
                {
                    configure(app);
                }
                app.Use(async (context, next) =>
                {
                    if (handler == null || !handler(context))
                    {
                        await next();
                    }
                });
            });
        }

        private static async Task<Transaction> SendAsync(TestServer server, string uri, string cookieHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.HttpClient.SendAsync(request),
            };
            if (transaction.Response.Headers.Contains("Set-Cookie"))
            {
                transaction.SetCookie = transaction.Response.Headers.GetValues("Set-Cookie").ToList();
            }
            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            return transaction;
        }

        private class Transaction
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }
            public IList<string> SetCookie { get; set; }
            public string ResponseText { get; set; }
        }
    }
}
#pragma warning restore 618