// <copyright file="RequestBodyTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else
namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class RequestBodyTests : TestBase
    {
        public void ReadBodyTwiceViaSeekApp(IAppBuilder app)
        {
            app.Use(context =>
            {
                StreamReader reader = new StreamReader(context.Request.Body);
                string text = reader.ReadToEnd();
                context.Response.WriteAsync(text).Wait();
                Assert.True(context.Request.Body.CanSeek);
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                reader = new StreamReader(context.Request.Body);
                text = reader.ReadToEnd();
                return context.Response.WriteAsync(text);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ReadBodyTwiceViaSeek(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ReadBodyTwiceViaSeekApp);

            var client = new HttpClient();
            return client.PostAsync("http://localhost:" + port, new StringContent("Hello World")).Then(result =>
            {
                Assert.Equal("Hello WorldHello World", result.Content.ReadAsStringAsync().Result);
            });
        }

        public void ReadBodyTwiceViaPositionApp(IAppBuilder app)
        {
            app.Use(context =>
            {
                StreamReader reader = new StreamReader(context.Request.Body);
                string text = reader.ReadToEnd();
                context.Response.WriteAsync(text).Wait();
                Assert.True(context.Request.Body.CanSeek);
                context.Request.Body.Position = 0;
                reader = new StreamReader(context.Request.Body);
                text = reader.ReadToEnd();
                return context.Response.WriteAsync(text);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ReadBodyTwiceViaPosition(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ReadBodyTwiceViaPositionApp);

            var client = new HttpClient();
            return client.PostAsync("http://localhost:" + port, new StringContent("Hello World")).Then(result =>
            {
                Assert.Equal("Hello WorldHello World", result.Content.ReadAsStringAsync().Result);
            });
        }

#if !NET40
        public void DisableRequestBufferingApp(IAppBuilder app)
        {
            app.Use(context =>
            {
                Assert.True(context.Request.Body.CanSeek);
                context.Get<Action>("server.DisableRequestBuffering")();
                Assert.False(context.Request.Body.CanSeek);
                StreamReader reader = new StreamReader(context.Request.Body);
                string text = reader.ReadToEnd();
                Assert.False(context.Request.Body.CanSeek);
                return context.Response.WriteAsync(text);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task DisableRequestBuffering(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DisableRequestBufferingApp);

            var client = new HttpClient();
            return client.PostAsync("http://localhost:" + port, new StringContent("Hello World")).Then(result =>
            {
                Assert.Equal("Hello World", result.Content.ReadAsStringAsync().Result);
            });
        }
#endif
    }
}
