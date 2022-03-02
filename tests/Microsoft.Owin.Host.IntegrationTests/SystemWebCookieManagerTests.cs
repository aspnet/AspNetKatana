// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Host.SystemWeb;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class SystemWebCookieManagerTests : TestBase
    {
        internal void CookieManagerEchoCookieApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                var cookieManager = new SystemWebCookieManager();
                var cookie = cookieManager.GetRequestCookie(context, context.Request.Headers["CookieName"]);
                return context.Response.WriteAsync(cookie ?? "null");
            });
        }

        [Theory]
        [InlineData("name", "name=value", "value")]
        [InlineData("name123", "name123=value456", "value456")]
        [InlineData("name+!@#$%^&*()_", "name%2B%21%40%23%24%25%5E%26%2A%28%29_=value%2B%21%40%23%24%25%5E%26%2A%28%29_", "value+!@#$%^&*()_")]
        public async Task EchoCookie(string cookieName, string value, string expected)
        {
            int port = RunWebServer("Microsoft.Owin.Host.SystemWeb", CookieManagerEchoCookieApp);

            var client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:" + port + "/EchoCookie");
            message.Headers.TryAddWithoutValidation("CookieName", cookieName);
            message.Headers.TryAddWithoutValidation("Cookie", value);
            var response = await client.SendAsync(message);
            Assert.Equal(expected, await response.Content.ReadAsStringAsync());
        }

        internal void CookieManagerAppendCookieApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                var cookieManager = new SystemWebCookieManager();
                cookieManager.AppendResponseCookie(
                    context, context.Request.Headers["CookieName"], context.Request.Headers["CookieValue"], new CookieOptions());
                return context.Response.WriteAsync("AppendCookieApp");
            });
        }

        [Theory]
        [InlineData("name", "value", "name=value; path=/")]
        [InlineData("name123", "value456", "name123=value456; path=/")]
        [InlineData("name+!@#$%^&*()_", "value+!@#$%^&*()_", "name%2B%21%40%23%24%25%5E%26%2A%28%29_=value%2B%21%40%23%24%25%5E%26%2A%28%29_; path=/")]
        public async Task AppendCookie(string cookieName, string value, string expected)
        {
            int port = RunWebServer("Microsoft.Owin.Host.SystemWeb", CookieManagerAppendCookieApp);

            var client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:" + port + "/AppendCookie");
            message.Headers.TryAddWithoutValidation("CookieName", cookieName);
            message.Headers.TryAddWithoutValidation("CookieValue", value);
            var response = await client.SendAsync(message);
            Assert.Equal("AppendCookieApp", await response.Content.ReadAsStringAsync());
            IEnumerable<string> values;
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out values));
            Assert.Single(values);
            Assert.Equal(expected, values.First());
        }

        internal void CookieManagerDeleteCookieApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                var cookieManager = new SystemWebCookieManager();
                cookieManager.DeleteCookie(context, context.Request.Headers["CookieName"], new CookieOptions());
                return context.Response.WriteAsync("DeleteCookieApp");
            });
        }

        [Theory]
        [InlineData("name", "name=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/")]
        [InlineData("name123", "name123=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/")]
        [InlineData("name+!@#$%^&*()_", "name%2B%21%40%23%24%25%5E%26%2A%28%29_=; expires=Thu, 01-Jan-1970 00:00:00 GMT; path=/")]
        public async Task DeleteCookie(string cookieName, string expected)
        {
            int port = RunWebServer("Microsoft.Owin.Host.SystemWeb", CookieManagerDeleteCookieApp);

            var client = new HttpClient(new HttpClientHandler() { UseCookies = false });
            var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost:" + port + "/DeleteCookie");
            message.Headers.TryAddWithoutValidation("CookieName", cookieName);
            var response = await client.SendAsync(message);
            Assert.Equal("DeleteCookieApp", await response.Content.ReadAsStringAsync());
            IEnumerable<string> values;
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out values));
            Assert.Single(values);
            Assert.Equal(expected, values.First());
        }
    }
}
