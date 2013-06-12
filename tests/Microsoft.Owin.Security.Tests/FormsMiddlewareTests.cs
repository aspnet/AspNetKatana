using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.Forms;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Security.Tests
{
    public class FormsMiddlewareTests
    {
        [Fact]
        public async Task NormalRequestPassesThrough()
        {
            var server = CreateServer(new FormsAuthenticationOptions
            {
            });
            var response = await server.HttpClient.GetAsync("http://example.com/normal");
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ProtectedRequestShouldRedirectToLogin()
        {
            var server = CreateServer(new FormsAuthenticationOptions
            {
                LoginPath = "/login"
            });

            var response = await server.HttpClient.GetAsync("http://example.com/protected");

            response.StatusCode.ShouldBe(HttpStatusCode.Redirect);

            Uri location = response.Headers.Location;
            location.LocalPath.ShouldBe("/login");
            location.Query.ShouldBe("?ReturnUrl=%2Fprotected");
        }

        private async Task SignInAsAlice(OwinRequest req, OwinResponse res)
        {
            req.Authentication.SignIn(
                new ClaimsPrincipal(new GenericIdentity("Alice", "Forms")),
                new AuthenticationExtra());
        }

        [Fact]
        public async Task SignInCausesDefaultCookieToBeCreated()
        {
            var server = CreateServer(new FormsAuthenticationOptions
            {
                LoginPath = "/login",
                CookieName = "TestCookie",
            }, SignInAsAlice);

            var response = await server.HttpClient.GetAsync("http://example.com/testpath");
            string setCookie = response.Headers.GetValues("Set-Cookie").Single();
            setCookie.ShouldStartWith("TestCookie=");
            setCookie.ShouldContain("; path=/");
            setCookie.ShouldContain("; HttpOnly");
            setCookie.ShouldNotContain("; expires=");
            setCookie.ShouldNotContain("; domain=");
            setCookie.ShouldNotContain("; secure");
        }

        [Theory]
        [InlineData(CookieSecureOption.Always, "http://example.com/testpath", true)]
        [InlineData(CookieSecureOption.Always, "https://example.com/testpath", true)]
        [InlineData(CookieSecureOption.Never, "http://example.com/testpath", false)]
        [InlineData(CookieSecureOption.Never, "https://example.com/testpath", false)]
        [InlineData(CookieSecureOption.SameAsRequest, "http://example.com/testpath", false)]
        [InlineData(CookieSecureOption.SameAsRequest, "https://example.com/testpath", true)]
        public async Task SecureSignInCausesSecureOnlyCookieByDefault(
            CookieSecureOption cookieSecureOption,
            string requestUri,
            bool shouldBeSecureOnly)
        {
            var server = CreateServer(new FormsAuthenticationOptions
            {
                LoginPath = "/login",
                CookieName = "TestCookie",
                CookieSecure = cookieSecureOption
            }, SignInAsAlice);

            var response = await server.HttpClient.GetAsync(requestUri);
            string setCookie = response.Headers.GetValues("Set-Cookie").Single();

            if (shouldBeSecureOnly)
            {
                setCookie.ShouldContain("; secure");
            }
            else
            {
                setCookie.ShouldNotContain("; secure");
            }
        }

        [Fact]
        public async Task CookieOptionsAlterSetCookieHeader()
        {
            var server1 = CreateServer(new FormsAuthenticationOptions
            {
                CookieName = "TestCookie",
                CookiePath = "/foo",
                CookieDomain = "another.com",
                CookieSecure = CookieSecureOption.Always,
                CookieHttpOnly = true,
            }, SignInAsAlice);

            var response1 = await server1.HttpClient.GetAsync("http://example.com/testpath");

            var server2 = CreateServer(new FormsAuthenticationOptions
            {
                CookieName = "SecondCookie",
                CookieSecure = CookieSecureOption.Never,
                CookieHttpOnly = false,
            }, SignInAsAlice);

            var response2 = await server2.HttpClient.GetAsync("http://example.com/testpath");

            string setCookie1 = response1.Headers.GetValues("Set-Cookie").Single();
            string setCookie2 = response2.Headers.GetValues("Set-Cookie").Single();

            setCookie1.ShouldContain("TestCookie=");
            setCookie1.ShouldContain(" path=/foo");
            setCookie1.ShouldContain(" domain=another.com");
            setCookie1.ShouldContain(" secure");
            setCookie1.ShouldContain(" HttpOnly");

            setCookie2.ShouldContain("SecondCookie=");
            setCookie2.ShouldNotContain(" domain=");
            setCookie2.ShouldNotContain(" secure");
            setCookie2.ShouldNotContain(" HttpOnly");
        }

        [Fact]
        public async Task CookieContainsIdentity()
        {
            var clock = new TestClock();
            var server = CreateServer(new FormsAuthenticationOptions
            {
                SystemClock = clock
            }, SignInAsAlice);

            var cookie = await GetReturnedCookie(server);

            var me = await GetAuthData(server, "http://example.com/me/Forms", cookie);

            XElement nameClaim = me.Elements("claim").Single(elt => elt.Attribute("type").Value == ClaimTypes.Name);
            nameClaim.Attribute("value").Value.ShouldBe("Alice");
        }

        private static async Task<string> GetReturnedCookie(TestServer server)
        {
            var response = await server.HttpClient.GetAsync("http://example.com/testpath");

            var cookie = response.Headers.GetValues("Set-Cookie").Single().Split(';').First();
            return cookie;
        }

        private static async Task<XElement> GetAuthData(TestServer server, string url, string cookie)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", cookie);

            var response2 = await server.HttpClient.SendAsync(request);
            string text = await response2.Content.ReadAsStringAsync();
            XElement me = XElement.Parse(text);
            return me;
        }

        private static TestServer CreateServer(FormsAuthenticationOptions options, Func<OwinRequest, OwinResponse, Task> testpath = null)
        {
            return TestServer.Create(app =>
            {
                app.Properties["host.AppName"] = "Microsoft.Owin.Security.Tests";
                app.UseFormsAuthentication(options);
                app.UseHandler(async (req, res, next) =>
                {
                    if (req.Path == "/normal")
                    {
                        res.StatusCode = 200;
                    }
                    else if (req.Path == "/protected")
                    {
                        res.StatusCode = 401;
                    }
                    else if (req.Path == "/me")
                    {
                        Describe(res, new AuthenticateResult(req.User.Identity, new Dictionary<string, string>(), new Dictionary<string, object>()));
                    }
                    else if (req.Path.StartsWith("/me/"))
                    {
                        var identity = await req.Authentication.AuthenticateAsync(req.Path.Substring("/me/".Length));
                        Describe(res, identity);
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

        private static void Describe(OwinResponse res, AuthenticateResult result)
        {
            res.StatusCode = 200;
            res.ContentType = "text/xml";
            var xml = new XElement("xml");
            xml.Add(result.Identity.Claims.Select(claim => new XElement("claim", new XAttribute("type", claim.Type), new XAttribute("value", claim.Value))));
            xml.Add(result.Extra.Properties.Select(extra => new XElement("extra", new XAttribute("type", extra.Key), new XAttribute("value", extra.Value))));
            using (var memory = new MemoryStream())
            {
                using (var writer = new XmlTextWriter(memory, Encoding.UTF8))
                {
                    xml.WriteTo(writer);
                }
                res.Body.Write(memory.ToArray(), 0, memory.ToArray().Length);
            }
        }
    }

    public class TestClock : ISystemClock
    {
        public TestClock()
        {
            UtcNow = new DateTimeOffset(2013, 6, 11, 12, 34, 56, 789, TimeSpan.Zero);
        }

        public DateTimeOffset UtcNow { get; set; }

        public void Add(TimeSpan timeSpan)
        {
            UtcNow = UtcNow + timeSpan;
        }
    }
}
