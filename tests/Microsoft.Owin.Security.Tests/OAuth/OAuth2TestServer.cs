// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Owin;
using Shouldly;

namespace Microsoft.Owin.Security.Tests.OAuth
{
    public class OAuth2TestServer : TestServer
    {
        public OAuth2TestServer(Action<OAuth2TestServer> configure = null)
        {
            var clock = new TestClock();
            Options = new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = new PathString("/authorize"),
                TokenEndpointPath = new PathString("/token"),
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnGrantAuthorizationCode = ctx =>
                    {
                        if (ctx.Ticket != null && ctx.Ticket.Identity != null && ctx.Ticket.Identity.IsAuthenticated)
                        {
                            ctx.Validated();
                        }
                        return Task.FromResult(0);
                    },
                    OnGrantRefreshToken = ctx =>
                    {
                        if (ctx.Ticket != null && ctx.Ticket.Identity != null && ctx.Ticket.Identity.IsAuthenticated)
                        {
                            ctx.Validated();
                        }
                        return Task.FromResult(0);
                    },
                    OnValidateClientRedirectUri = ctx =>
                    {
                        if (ctx.ClientId == "alpha")
                        {
                            ctx.Validated("https://gamma.com/return");
                        }
                        else if (ctx.ClientId == "alpha2")
                        {
                            ctx.Validated("https://gamma2.com/return");
                        }
                        else if (ctx.ClientId == "alpha3")
                        {
                            ctx.Validated("https://gamma3.com/return");
                        }
                        else if (ctx.ClientId == "unauthorized")
                        {
                            ctx.Validated("https://gamma.com/return");
                        }
                        return Task.FromResult(0);
                    },
                    OnValidateClientAuthentication = ctx =>
                    {
                        string clientId;
                        string clientSecret;
                        if (ctx.TryGetBasicCredentials(out clientId, out clientSecret) ||
                            ctx.TryGetFormCredentials(out clientId, out clientSecret))
                        {
                            if (clientId == "alpha" && clientSecret == "beta")
                            {
                                ctx.Validated();
                            }
                            else if (clientId == "alpha2" && clientSecret == "beta2")
                            {
                                ctx.Validated();
                            }
                            else if (clientId == "alpha3" && String.IsNullOrEmpty(clientSecret))
                            {
                                ctx.Validated();
                            }
                        }
                        return Task.FromResult(0);
                    },
                    OnValidateAuthorizeRequest = ctx =>
                    {
                        if (ctx.AuthorizeRequest.ClientId == "unauthorized")
                        {
                            ctx.SetError("unauthorized_client", "Unknown client", "https://owintestoauth.com/error/unknown_client");
                        }
                        else
                        {
                            ctx.Validated();
                        }
                        return Task.FromResult(0);
                    }
                },
                AuthorizationCodeProvider = new InMemorySingleUseReferenceProvider(),
                SystemClock = clock,
            };
            BearerOptions = new OAuthBearerAuthenticationOptions
            {
                Provider = new OAuthBearerAuthenticationProvider(),
                AccessTokenProvider = Options.AccessTokenProvider,
                SystemClock = clock,
            };
            if (configure != null)
            {
                configure(this);
            }
            Configure(app =>
            {
                app.Properties["host.AppName"] = "Microsoft.Owin.Security.Tests";
                app.UseOAuthBearerAuthentication(BearerOptions);
                app.UseOAuthAuthorizationServer(Options);
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path == Options.AuthorizeEndpointPath && OnAuthorizeEndpoint != null)
                    {
                        await OnAuthorizeEndpoint(ctx);
                    }
                    else if (ctx.Request.Path == new PathString("/testpath") && OnTestpathEndpoint != null)
                    {
                        await OnTestpathEndpoint(ctx);
                    }
                    else if (ctx.Request.Path == new PathString("/me"))
                    {
                        await MeEndpoint(ctx);
                    }
                    else
                    {
                        await next();
                    }
                });
            });
        }

        public OAuthAuthorizationServerOptions Options { get; set; }
        public OAuthBearerAuthenticationOptions BearerOptions { get; set; }

        public OAuthAuthorizationServerProvider Provider
        {
            get { return Options.Provider as OAuthAuthorizationServerProvider; }
        }

        public OAuthBearerAuthenticationProvider BearerProvider
        {
            get { return BearerOptions.Provider as OAuthBearerAuthenticationProvider; }
        }

        public TestClock Clock
        {
            get { return Options.SystemClock as TestClock; }
        }

        public Func<IOwinContext, Task> OnAuthorizeEndpoint { get; set; }
        public Func<IOwinContext, Task> OnTestpathEndpoint { get; set; }

        public async Task<Transaction> SendAsync(
            string uri,
            string cookieHeader = null,
            string postBody = null,
            AuthenticationHeaderValue authenticateHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
            if (authenticateHeader != null)
            {
                request.Headers.Authorization = authenticateHeader;
            }
            if (!string.IsNullOrEmpty(postBody))
            {
                request.Method = HttpMethod.Post;
                request.Content = new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            }

            var transaction = new Transaction
            {
                Request = request,
                Response = await HttpClient.SendAsync(request),
            };
            if (transaction.Response.Headers.Contains("Set-Cookie"))
            {
                transaction.SetCookie = transaction.Response.Headers.GetValues("Set-Cookie").SingleOrDefault();
            }
            if (!string.IsNullOrEmpty(transaction.SetCookie))
            {
                transaction.CookieNameValue = transaction.SetCookie.Split(new[] { ';' }, 2).First();
            }
            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "application/json")
            {
                transaction.ResponseToken = JToken.Parse(transaction.ResponseText);
            }

            return transaction;
        }

        private Task MeEndpoint(IOwinContext ctx)
        {
            if (ctx.Request.User == null ||
                !ctx.Request.User.Identity.IsAuthenticated ||
                ctx.Request.User.Identity.AuthenticationType != "Bearer")
            {
                ctx.Authentication.Challenge(new AuthenticationProperties(), "Bearer");
            }
            else
            {
                ctx.Response.Write(ctx.Request.User.Identity.Name);
            }

            return Task.FromResult<object>(null);
        }

        public class InMemorySingleUseReferenceProvider : AuthenticationTokenProvider
        {
            private readonly ConcurrentDictionary<string, string> _database = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

            public override void Create(AuthenticationTokenCreateContext context)
            {
                string tokenValue = Guid.NewGuid().ToString("n");

                _database[tokenValue] = context.SerializeTicket();

                context.SetToken(tokenValue);
            }

            public override void Receive(AuthenticationTokenReceiveContext context)
            {
                string value;
                if (_database.TryRemove(context.Token, out value))
                {
                    context.DeserializeTicket(value);
                }
            }
        }

        public class Transaction
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }

            public string SetCookie { get; set; }
            public string CookieNameValue { get; set; }

            public string ResponseText { get; set; }
            public XElement ResponseElement { get; set; }
            public JToken ResponseToken { get; set; }

            public NameValueCollection ParseRedirectQueryString()
            {
                Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
                Response.Headers.Location.Query.ShouldStartWith("?");
                string querystring = Response.Headers.Location.Query.Substring(1);
                var nvc = new NameValueCollection();
                foreach (var pair in querystring
                    .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split(new[] { '=' }, 2).Select(Uri.UnescapeDataString)))
                {
                    if (pair.Count() == 2)
                    {
                        nvc.Add(pair.First(), pair.Last());
                    }
                }
                return nvc;
            }

            public NameValueCollection ParseRedirectFragment()
            {
                Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
                Response.Headers.Location.Fragment.ShouldStartWith("#");
                string fragment = Response.Headers.Location.Fragment.Substring(1);
                var nvc = new NameValueCollection();
                foreach (var pair in fragment
                    .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split(new[] { '=' }, 2).Select(Uri.UnescapeDataString)))
                {
                    if (pair.Count() == 2)
                    {
                        nvc.Add(pair.First(), pair.Last());
                    }
                }
                return nvc;
            }
        }
    }
}
