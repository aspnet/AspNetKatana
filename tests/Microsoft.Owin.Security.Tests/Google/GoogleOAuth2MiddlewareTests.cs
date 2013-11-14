// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests.Google
{
    public class GoogleOAuth2MiddlewareTests
    {
        private const string CookieAuthenticationType = "Cookie";
        [Fact]
        public async Task ChallengeWillTriggerRedirection()
        {
            var server = CreateServer(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret"
            });
            var transaction = await SendAsync(server, "https://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            var location = transaction.Response.Headers.Location.ToString();
            location.ShouldContain("https://accounts.google.com/o/oauth2/auth?response_type=code");
            location.ShouldContain("&client_id=");
            location.ShouldContain("&redirect_uri=");
            location.ShouldContain("&scope=");
            location.ShouldContain("&state=");
        }

        [Fact]
        public async Task ChallengeWillSetCorrelationCookie()
        {
            var server = CreateServer(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret"
            });
            var transaction = await SendAsync(server, "https://example.com/challenge");
            Console.WriteLine(transaction.SetCookie);
            transaction.SetCookie.Single().ShouldContain(".AspNet.Correlation.Google=");
        }

        [Fact]
        public async Task ChallengeWillSetDefaultScope()
        {
            var server = CreateServer(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret"
            });
            var transaction = await SendAsync(server, "https://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            var query = transaction.Response.Headers.Location.Query;
            query.ShouldContain("&scope=" + Uri.EscapeDataString("openid profile email"));
        }

        [Fact]
        public async Task ChallengeWillUseOptionsScope()
        {
            var options = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret"
            };
            options.Scope.Add("https://www.googleapis.com/auth/plus.login");
            var server = CreateServer(options);
            var transaction = await SendAsync(server, "https://example.com/challenge");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            var query = transaction.Response.Headers.Location.Query;
            query.ShouldContain("&scope=" + Uri.EscapeDataString("https://www.googleapis.com/auth/plus.login"));
        }

        [Fact]
        public async Task ReplyPathWithoutStateQueryStringWillBeRejected()
        {
            var options = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret"
            };
            var server = CreateServer(options);
            var transaction = await SendAsync(server, "https://example.com/signin-google?code=TestCode");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task ReplyPathWillAuthenticateValidAuthorizeCodeAndState()
        {
            var options = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret",
                BackchannelHttpHandler = new TestHttpMessageHandler
                {
                    Sender = async req =>
                        {
                            if (req.RequestUri.AbsoluteUri == "https://accounts.google.com/o/oauth2/token")
                            {
                                return await ReturnJsonResponse(new
                                {
                                    access_token = "Test Access Token",
                                    expire_in = 3600,
                                    token_type = "Bearer"
                                });
                            }
                            else if (req.RequestUri.GetLeftPart(UriPartial.Path) == "https://www.googleapis.com/oauth2/v3/userinfo")
                            {
                                return await ReturnJsonResponse(new
                                {
                                    sub = "Test User ID",
                                    name = "Test Name",
                                    given_name = "Test Given Name",
                                    family_name = "Test Family Name",
                                    profile = "Profile link",
                                    email = "Test email"
                                });
                            }

                            return null;
                        }
                }
            };
            var server = CreateServer(options);
            var properties = new AuthenticationProperties();
            var correlationKey = ".AspNet.Correlation.Google";
            var correlationValue = "TestCorrelationId";
            properties.Dictionary.Add(correlationKey, correlationValue);
            properties.RedirectUri = "/me";
            var state = options.StateDataFormat.Protect(properties);
            var transaction = await SendAsync(server, 
                "https://example.com/signin-google?code=TestCode&state=" + Uri.EscapeDataString(state),
                correlationKey + "=" + correlationValue);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.ToString().ShouldBe("/me");
            transaction.SetCookie.Count.ShouldBe(2);
            transaction.SetCookie[0].ShouldContain(correlationKey);
            transaction.SetCookie[1].ShouldContain(".AspNet.Cookie");

            var authCookie = transaction.AuthenticationCookieValue;
            transaction = await SendAsync(server, "https://example.com/me", authCookie);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction.FindClaimValue(ClaimTypes.Name).ShouldBe("Test Name");
            transaction.FindClaimValue(ClaimTypes.NameIdentifier).ShouldBe("Test User ID");
            transaction.FindClaimValue(ClaimTypes.GivenName).ShouldBe("Test Given Name");
            transaction.FindClaimValue(ClaimTypes.Surname).ShouldBe("Test Family Name");
            transaction.FindClaimValue(ClaimTypes.Email).ShouldBe("Test email");
        }

        [Fact]
        public async Task ReplyPathWillRejectIfCodeIsInvalid()
        {
            var options = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret",
                BackchannelHttpHandler = new TestHttpMessageHandler
                {
                    Sender = req =>
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
                    }
                }
            };
            var server = CreateServer(options);
            var properties = new AuthenticationProperties();
            var correlationKey = ".AspNet.Correlation.Google";
            var correlationValue = "TestCorrelationId";
            properties.Dictionary.Add(correlationKey, correlationValue);
            properties.RedirectUri = "/me";
            var state = options.StateDataFormat.Protect(properties);
            var transaction = await SendAsync(server,
                "https://example.com/signin-google?code=TestCode&state=" + Uri.EscapeDataString(state),
                correlationKey + "=" + correlationValue);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.ToString().ShouldContain("error=access_denied");
        }

        [Fact]
        public async Task ReplyPathWillRejectIfAccessTokenIsMissing()
        {
            var options = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "Test Id",
                ClientSecret = "Test Secret",
                BackchannelHttpHandler = new TestHttpMessageHandler
                {
                    Sender = async req =>
                    {
                        return await ReturnJsonResponse(new object());
                    }
                }
            };
            var server = CreateServer(options);
            var properties = new AuthenticationProperties();
            var correlationKey = ".AspNet.Correlation.Google";
            var correlationValue = "TestCorrelationId";
            properties.Dictionary.Add(correlationKey, correlationValue);
            properties.RedirectUri = "/me";
            var state = options.StateDataFormat.Protect(properties);
            var transaction = await SendAsync(server,
                "https://example.com/signin-google?code=TestCode&state=" + Uri.EscapeDataString(state),
                correlationKey + "=" + correlationValue);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.ToString().ShouldContain("error=access_denied");
        }


        private static async Task<HttpResponseMessage> ReturnJsonResponse(object content)
        {
            var res = new HttpResponseMessage(HttpStatusCode.OK);
            var text = await JsonConvert.SerializeObjectAsync(content);
            res.Content = new StringContent(text, Encoding.UTF8, "application/json");
            return res;
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

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }
            return transaction;
        }

        private static TestServer CreateServer(GoogleOAuth2AuthenticationOptions options, Func<IOwinContext, Task> testpath = null)
        {
            return TestServer.Create(app =>
            {
                app.Properties["host.AppName"] = "Microsoft.Owin.Security.Tests";
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationType = CookieAuthenticationType
                    });
                options.SignInAsAuthenticationType = CookieAuthenticationType;
                IDataProtector dataProtecter = app.CreateDataProtector(
                    typeof(GoogleOAuth2AuthenticationMiddleware).FullName,
                    options.AuthenticationType, "v1");
                options.StateDataFormat = new PropertiesDataFormat(dataProtecter);
                app.UseGoogleAuthentication(options);
                app.Use(async (context, next) =>
                {
                    IOwinRequest req = context.Request;
                    IOwinResponse res = context.Response;
                    PathString remainder;
                    if (req.Path == new PathString("/challenge"))
                    {
                        context.Authentication.Challenge("Google");
                        res.StatusCode = 401;
                    }
                    else if (req.Path == new PathString("/me"))
                    {
                        Describe(res, new AuthenticateResult(req.User.Identity, new AuthenticationProperties(), new AuthenticationDescription()));
                    }
                    else
                    {
                        await next();
                    }
                });
            });
        }

        private static void Describe(IOwinResponse res, AuthenticateResult result)
        {
            res.StatusCode = 200;
            res.ContentType = "text/xml";
            var xml = new XElement("xml");
            if (result != null && result.Identity != null)
            {
                xml.Add(result.Identity.Claims.Select(claim => new XElement("claim", new XAttribute("type", claim.Type), new XAttribute("value", claim.Value))));
            }
            if (result != null && result.Properties != null)
            {
                xml.Add(result.Properties.Dictionary.Select(extra => new XElement("extra", new XAttribute("type", extra.Key), new XAttribute("value", extra.Value))));
            }
            using (var memory = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(memory, Encoding.UTF8))
                {
                    xml.WriteTo(writer);
                }
                res.Body.Write(memory.ToArray(), 0, memory.ToArray().Length);
            }
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, Task<HttpResponseMessage>> Sender { get; set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                if (Sender != null)
                {
                    return await Sender(request);
                }

                return null;
            }
        }
        private class Transaction
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }

            public IList<string> SetCookie { get; set; }

            public string ResponseText { get; set; }
            public XElement ResponseElement { get; set; }

            public string AuthenticationCookieValue
            {
                get
                {
                    if (SetCookie != null && SetCookie.Count > 0)
                    {
                        var authCookie = SetCookie.SingleOrDefault(c => c.Contains(".AspNet.Cookie="));
                        if (authCookie != null)
                        {
                            return authCookie.Substring(0, authCookie.IndexOf(';'));
                        }
                    }

                    return null;
                }
            }

            public string FindClaimValue(string claimType)
            {
                XElement claim = ResponseElement.Elements("claim").SingleOrDefault(elt => elt.Attribute("type").Value == claimType);
                if (claim == null)
                {
                    return null;
                }
                return claim.Attribute("value").Value;
            }
        }

    }
}
