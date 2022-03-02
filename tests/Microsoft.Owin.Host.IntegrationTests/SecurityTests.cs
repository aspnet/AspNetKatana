﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class SecurityTests : TestBase
    {
        public void SetCustomUser(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Request.User = new GenericPrincipal(new GenericIdentity("Bob"), null);
                return context.Response.WriteAsync(Thread.CurrentPrincipal.Identity.Name);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task SetUser_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetCustomUser);

            var client = new HttpClient();
            var result = await client.GetStringAsync("http://localhost:" + port + "/custom");
            Assert.Equal("Bob", result);
        }
    }
}
