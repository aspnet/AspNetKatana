using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Owin.Security.Forms;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class OAuth2AuthorizationServerTests
    {
        [Fact]
        public async Task MissingClientIdDoesNotRedirect()
        {
            TestServer server = CreateServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/authorize"
            });

            var transaction = await SendAsync(server, "http://example.com/authorize");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task MissingIncorrectRedirectUriDoesNotRedirect()
        {
            TestServer server = CreateServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/authorize",
                Provider = new OAuthAuthorizationServerProvider
                {
                    OnValidateClientCredentials = async ctx => ctx.ClientFound("beta", "http://gamma.com/return")
                }
            });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return2"));

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task MissingResponseTypeRedirectsWithErrorMessage()
        {
            TestServer server = CreateServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/authorize",
                Provider = CreateProvider()
            });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=alpha");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.Query.ShouldContain("error=invalid_request");
        }

        [Fact]
        public async Task UnsupportedResponseTypeRedirectsWithErrorMessage()
        {
            TestServer server = CreateServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/authorize",
                Provider = CreateProvider()
            });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=alpha&response_type=delta");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.Query.ShouldContain("error=unsupported_response_type");
        }

        [Fact]
        public async Task AuthorizeRequestMayPassThroughToApplicationRequestHandler()
        {
            TestServer server = CreateServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/authorize",
                Provider = CreateProvider()
            },
            async (req, res) =>
            {
                res.ContentType = "text/plain";
                using (var writer = new StreamWriter(res.Body, Encoding.UTF8, 4096, leaveOpen: true))
                {
                    await writer.WriteAsync("Responding");
                }
            });
            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=beta&response_type=code");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction.ResponseText.ShouldBe("Responding");
        }

        [Fact]
        public async Task CallingSignInWillRedirectWithAuthorizationCode()
        {
            TestServer server = CreateServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/authorize",
                Provider = CreateProvider()
            },
            async (req, res) =>
            {
                req.Authentication.SignIn(
                    new AuthenticationExtra(),
                    CreateIdentity("epsilon"));
            });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=beta&response_type=code");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.Query.ShouldContain("code=");
        }

        [Fact]
        public async Task NonTwoHundredDoesNotGetChanged()
        {
            TestServer server = CreateServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = "/authorize",
                Provider = CreateProvider()
            },
            async (req, res) =>
            {
                res.StatusCode = 404;
                req.Authentication.SignIn(
                    new AuthenticationExtra(),
                    CreateIdentity("epsilon"));
            });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=beta&response_type=code");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            transaction.Response.Headers.Location.ShouldBe(null);
        }

        [Fact]
        public async Task CodeCanBeExchangedForToken()
        {
            TestServer server = CreateServer(
                new OAuthAuthorizationServerOptions
                {
                    AuthorizeEndpointPath = "/authorize",
                    TokenEndpointPath = "/token",
                    Provider = CreateProvider()
                },
                async (req, res) =>
                {
                    req.Authentication.SignIn(
                        new AuthenticationExtra(),
                        CreateIdentity("epsilon"));
                });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=beta&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await SendAsync(server, "http://example.com/token", postBody:
                "grant_type=authorization_code&code=" + query["code"] + "&client_id=beta");

            transaction2.ResponseToken["access_token"].Value<string>().ShouldNotBe(null);
            transaction2.ResponseToken["token_type"].Value<string>().ShouldBe("bearer");
        }

        [Fact]
        public async Task CodeExpiresAfterGivenTimespan()
        {
            var clock = new TestClock();

            TestServer server = CreateServer(
                new OAuthAuthorizationServerOptions
                {
                    AuthorizeEndpointPath = "/authorize",
                    TokenEndpointPath = "/token",
                    Provider = CreateProvider(),
                    AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(8),
                    SystemClock = clock
                },
                async (req, res) =>
                {
                    req.Authentication.SignIn(
                        new AuthenticationExtra(),
                        CreateIdentity("epsilon"));
                });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=gamma&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            clock.Add(TimeSpan.FromMinutes(10));

            var transaction2 = await SendAsync(server, "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("gamma:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=gamma");

            transaction2.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }

        [Fact]
        public async Task TokenTellsYouHowManySecondsItWillExpireIn()
        {
            var clock = new TestClock();

            TestServer server = CreateServer(
                new OAuthAuthorizationServerOptions
                {
                    AuthorizeEndpointPath = "/authorize",
                    TokenEndpointPath = "/token",
                    Provider = CreateProvider(),
                    AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(8),
                    AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(655321),
                    SystemClock = clock
                },
                async (req, res) =>
                {
                    req.Authentication.SignIn(
                        new AuthenticationExtra(),
                        CreateIdentity("epsilon"));
                });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=gamma&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await SendAsync(server, "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("gamma:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=gamma");

            transaction2.ResponseToken["access_token"].Value<string>().ShouldNotBe(null);
            transaction2.ResponseToken["token_type"].Value<string>().ShouldBe("bearer");
            transaction2.ResponseToken["expires_in"].Value<long>().ShouldBe(655321);
        }

        [Fact]
        public async Task CodeCanBeUsedOnlyOneTime()
        {
            var clock = new TestClock();

            TestServer server = CreateServer(
                new OAuthAuthorizationServerOptions
                {
                    AuthorizeEndpointPath = "/authorize",
                    TokenEndpointPath = "/token",
                    Provider = CreateProvider(),
                    AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(8),
                    AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(655321),
                    SystemClock = clock
                },
                async (req, res) =>
                {
                    req.Authentication.SignIn(
                        new AuthenticationExtra(),
                        CreateIdentity("epsilon"));
                });

            var transaction = await SendAsync(server, "http://example.com/authorize?client_id=gamma&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await SendAsync(server, "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("gamma:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=gamma");

            transaction2.ResponseToken["access_token"].Value<string>().ShouldNotBe(null);
            transaction2.ResponseToken["token_type"].Value<string>().ShouldBe("bearer");
            transaction2.ResponseToken["expires_in"].Value<long>().ShouldBe(655321);

            var transaction3 = await SendAsync(server, "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("gamma:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=gamma");

            transaction3.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }

        private static ClaimsIdentity CreateIdentity(string name, params string[] scopes)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, name)
            };
            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope));
            }
            return new ClaimsIdentity(
                claims,
                "Bearer");
        }

        private static TestServer CreateServer(
            OAuthAuthorizationServerOptions options,
            Func<OwinRequest, OwinResponse, Task> authorize = null,
            Func<OwinRequest, OwinResponse, Task> testpath = null)
        {
            if (options.AuthenticationCodeProvider == null)
            {
                options.AuthenticationCodeProvider = new InMemorySingleUseReferenceProvider();
            }

            return TestServer.Create(app =>
            {
                app.Properties["host.AppName"] = "Microsoft.Owin.Security.Tests";
                app.UseOAuthAuthorizationServer(options);
                app.UseHandler(async (req, res, next) =>
                {
                    if (req.Path == options.AuthorizeEndpointPath && authorize != null)
                    {
                        await authorize(req, res);
                    }
                    else if (req.Path == "/testpath" && testpath != null)
                    {
                        await testpath(req, res);
                    }
                    else
                    {
                        await next();
                    }
                });
            });
        }

        private static OAuthAuthorizationServerProvider CreateProvider()
        {
            return new OAuthAuthorizationServerProvider
            {
                OnValidateClientCredentials = async ctx => ctx.ClientFound("beta", "http://gamma.com/return")
            };
        }

        private static async Task<Transaction> SendAsync(
            TestServer server,
            string uri,
            string cookieHeader = null,
            string postBody = null,
            AuthenticationHeaderValue authenticateHeader = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
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
                Response = await server.HttpClient.SendAsync(request),
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

        private class Transaction
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
                var querystring = Response.Headers.Location.Query.Substring(1);
                var nvc = new NameValueCollection();
                foreach (var pair in querystring
                    .Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries)
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

    public class InMemorySingleUseReferenceProvider : AuthenticationTicketProvider
    {
        readonly ConcurrentDictionary<string, string> _database = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        public override void Creating(AuthenticationTicketProviderContext context)
        {
            context.TokenValue = Guid.NewGuid().ToString("n");

            _database[context.TokenValue] = context.ProtectedData;
        }

        public override void Consuming(AuthenticationTicketProviderContext context)
        {
            string value;
            if (_database.TryRemove(context.TokenValue, out value))
            {
                context.SetProtectedData(value);
            }
        }
    }
}
