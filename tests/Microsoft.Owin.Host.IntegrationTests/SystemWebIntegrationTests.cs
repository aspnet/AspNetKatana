// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Routing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class SystemWebIntegrationTests : TestBase
    {
        public void ModuleAndHandlerEnvKeys(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use(async (context, next) =>
            {
                context.Set("test.IntegratedPipleine", "before");
                await next();
                Assert.Equal("after", context.Get<string>("test.IntegratedPipleine"));
            });

            RouteTable.Routes.MapOwinPath("/", app2 =>
            {
                app2.Run(context2 =>
                {
                    Assert.Equal("before", context2.Get<string>("test.IntegratedPipleine"));
                    context2.Set("test.IntegratedPipleine", "after");
                    return Task.FromResult(0);
                });
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ModuleAndHandlerEnvKeys_SharedEnv(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ModuleAndHandlerEnvKeys);

            return SendRequestAsync(port);
        }

        public void ExpectedKeys(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use((context, next) =>
            {
                var env = context.Environment;
                object ignored;
                Assert.True(env.TryGetValue("owin.RequestMethod", out ignored));
                Assert.Equal("GET", env["owin.RequestMethod"]);

                Assert.True(env.TryGetValue("owin.RequestPath", out ignored));
                Assert.Equal("/", env["owin.RequestPath"]);

                Assert.True(env.TryGetValue("owin.RequestPathBase", out ignored));
                Assert.Equal(string.Empty, env["owin.RequestPathBase"]);

                Assert.True(env.TryGetValue("owin.RequestProtocol", out ignored));
                Assert.Equal("HTTP/1.1", env["owin.RequestProtocol"]);

                Assert.True(env.TryGetValue("owin.RequestQueryString", out ignored));
                Assert.Equal(string.Empty, env["owin.RequestQueryString"]);

                Assert.True(env.TryGetValue("owin.RequestScheme", out ignored));
                Assert.Equal("http", env["owin.RequestScheme"]);

                Assert.True(env.TryGetValue("owin.Version", out ignored));
                Assert.Equal("1.0", env["owin.Version"]);

                Assert.True(env.TryGetValue("owin.RequestId", out ignored));
                Assert.False(string.IsNullOrWhiteSpace((string)env["owin.RequestId"]));

                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ExpectedKeys_Present(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ExpectedKeys);

            return SendRequestAsync(port);
        }

        public void ModuleAndHandlerSyncException(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use(async (context, next) =>
            {
                // Expect async exception from the handler.
                try
                {
                    await next();
                    Assert.True(false, "Handler exception expected");
                }
                catch (NotFiniteNumberException)
                {
                }
            });

            RouteTable.Routes.MapOwinPath("/", app2 =>
            {
                app2.Run(context2 =>
                {
                    // Sync exception should become async before module sees it.
                    throw new NotFiniteNumberException("Handler exception");
                });
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ModuleAndHandlerSyncException_ModuleSeesHandlerException(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ModuleAndHandlerSyncException);

            return SendRequestAsync(port);
        }

        public void ModuleAndHandlerAsyncException(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use(async (context, next) =>
            {
                try
                {
                    // Expect async exception from the handler.
                    await next();
                    Assert.True(false, "Handler exception expected");
                }
                catch (Exception ex)
                {
                    Assert.IsType<AggregateException>(ex);
                    Assert.IsType<NotFiniteNumberException>(ex.GetBaseException());
                }
            });

            RouteTable.Routes.MapOwinPath("/", app2 =>
            {
                app2.Run(context2 =>
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                    tcs.TrySetException(new NotFiniteNumberException("Handler exception"));
                    return tcs.Task;
                });
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ModuleAndHandlerAsyncException_ModuleSeesHandlerException(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ModuleAndHandlerAsyncException);

            return SendRequestAsync(port);
        }

        private async Task SendRequestAsync(int port)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port);
            Assert.Equal(String.Empty, await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
        }
    }
}
