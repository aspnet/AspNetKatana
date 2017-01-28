// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Host.SystemWeb;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class SystemWebChunkingCookieManagerTests : TestBase
    {
        public void ChunkingCookieManagerEchoCookieApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                var cookieManager = new SystemWebChunkingCookieManager();
                var cookie = cookieManager.GetRequestCookie(context, context.Request.Headers["CookieName"]);
                return context.Response.WriteAsync(cookie ?? "null");
            });
        }

        [Theory]
        [InlineData("name", "name=value", "value")]
        [InlineData("name123", "name123=value456", "value456")]
        [InlineData("name+!@#$%^&*()_", "name%2B%21%40%23%24%25%5E%26%2A%28%29_=value%2B%21%40%23%24%25%5E%26%2A%28%29_", "value+!@#$%^&*()_")]
        [InlineData("name+!", "name%2B%21=chunks:2; name%2B%21C1=value1%2; name%2B%21C2=Bvalue2", "value1+value2")]
        public async Task EchoCookie(string cookieName, string value, string expected)
        {
            int port = RunWebServer("Microsoft.Owin.Host.SystemWeb", ChunkingCookieManagerEchoCookieApp);

            var client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:" + port + "/EchoCookie");
            message.Headers.TryAddWithoutValidation("CookieName", cookieName);
            message.Headers.TryAddWithoutValidation("Cookie", value);
            var response = await client.SendAsync(message);
            Assert.Equal(expected, await response.Content.ReadAsStringAsync());
        }

        public void ChunkingCookieManagerAppendCookieApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                var cookieManager = new SystemWebChunkingCookieManager() { ChunkSize = 80 };
                cookieManager.AppendResponseCookie(
                    context, context.Request.Headers["CookieName"], context.Request.Headers["CookieValue"], new CookieOptions());
                return context.Response.WriteAsync("AppendCookieApp");
            });
        }

        [Theory]
        [InlineData("name", "value", "name=value; path=/")]
        [InlineData("name123", "value456", "name123=value456; path=/")]
        [InlineData("name+!@#$%^&*()_", "value+!@#$%^&*()_", "name%2B%21%40%23%24%25%5E%26%2A%28%29_=chunks:2; path=/; name%2B%21%40%23%24%25%5E%26%2A%28%29_C1=value%2B%21%40%23%24%25%5E%26%; path=/; name%2B%21%40%23%24%25%5E%26%2A%28%29_C2=2A%28%29_; path=/")]
        public async Task AppendCookie(string cookieName, string value, string expected)
        {
            int port = RunWebServer("Microsoft.Owin.Host.SystemWeb", ChunkingCookieManagerAppendCookieApp);

            var client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:" + port + "/AppendCookie");
            message.Headers.TryAddWithoutValidation("CookieName", cookieName);
            message.Headers.TryAddWithoutValidation("CookieValue", value);
            var response = await client.SendAsync(message);
            Assert.Equal("AppendCookieApp", await response.Content.ReadAsStringAsync());
            IEnumerable<string> values;
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out values));
            Assert.Equal(expected, string.Join("; ", values));
        }

        public void ChunkingCookieManagerDeleteCookieApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                var cookieManager = new SystemWebChunkingCookieManager();
                cookieManager.DeleteCookie(context, context.Request.Headers["CookieName"], new CookieOptions());
                return context.Response.WriteAsync("DeleteCookieApp");
            });
        }

        [Theory]
        [InlineData("name", "name=value", "name=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/")]
        [InlineData("name123", "name123=value123", "name123=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/")]
        [InlineData("name+!@#$%^&*()_", "name%2B%21%40%23%24%25%5E%26%2A%28%29_=chunks:2;",
            "name%2B%21%40%23%24%25%5E%26%2A%28%29_=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/; name%2B%21%40%23%24%25%5E%26%2A%28%29_C1=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/; name%2B%21%40%23%24%25%5E%26%2A%28%29_C2=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/")]
        public async Task DeleteCookie(string cookieName, string requestCookie, string expected)
        {
            int port = RunWebServer("Microsoft.Owin.Host.SystemWeb", ChunkingCookieManagerDeleteCookieApp);

            var client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:" + port + "/DeleteCookie");
            message.Headers.TryAddWithoutValidation("CookieName", cookieName);
            message.Headers.TryAddWithoutValidation("Cookie", requestCookie);
            var response = await client.SendAsync(message);
            Assert.Equal("DeleteCookieApp", await response.Content.ReadAsStringAsync());
            IEnumerable<string> values;
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out values));
            Assert.Equal(expected, string.Join("; ", values));
        }
    }
}
