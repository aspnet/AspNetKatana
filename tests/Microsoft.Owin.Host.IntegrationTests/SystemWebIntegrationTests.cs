// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Routing;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class SystemWebIntegrationTests : TestBase
    {
        public void ModuleAndHandlerEnvKeys(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use((context, next) =>
            {
                context.Set("test.IntegratedPipleine", "before");
                return next()
                    .Then(() => { Assert.Equal("after", context.Get<string>("test.IntegratedPipleine")); });
            });

            RouteTable.Routes.MapOwinPath("/", app2 =>
            {
                app2.Run(context2 =>
                {
                    Assert.Equal("before", context2.Get<string>("test.IntegratedPipleine"));
                    context2.Set("test.IntegratedPipleine", "after");
                    return TaskHelpers.Completed();
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
            app.Use((context, next) =>
            {
                // Expect async exception from the handler.
                return next()
                    .Then(() => { Assert.True(false, "Handler exception expected"); })
                    .Catch(catchInfo =>
                    {
                        Assert.IsType<NotFiniteNumberException>(catchInfo.Exception);
                        return catchInfo.Handled();
                    });
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
            app.Use((context, next) =>
            {
                // Expect async exception from the handler.
                return next()
                    .Then(() => { Assert.True(false, "Handler exception expected"); })
                    .Catch(catchInfo =>
                    {
                        Assert.IsType<NotFiniteNumberException>(catchInfo.Exception);
                        return catchInfo.Handled();
                    });
            });

            RouteTable.Routes.MapOwinPath("/", app2 => { app2.Run(context2 => { return TaskHelpers.FromError(new NotFiniteNumberException("Handler exception")); }); });
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

        private Task SendRequestAsync(int port)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port)
                         .Then(response =>
                         {
                             Assert.Equal(String.Empty, response.Content.ReadAsStringAsync().Result);
                             response.EnsureSuccessStatusCode();
                         });
        }
    }
}
