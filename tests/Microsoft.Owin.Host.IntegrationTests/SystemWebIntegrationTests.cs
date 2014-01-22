// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
