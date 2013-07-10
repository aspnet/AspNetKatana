// <copyright file="IntegratedPipelineTests.cs" company="Microsoft Open Technologies, Inc.">
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
    public class IntegratedPipelineTests : TestBase
    {
        public void NoStagesSpecified(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use<BreadCrumbMiddleware>("a", "PreHandlerExecute");
            app.Use<BreadCrumbMiddleware>("b", "PreHandlerExecute");
            app.Use<BreadCrumbMiddleware>("c", "PreHandlerExecute");
            app.Use(context =>
            {
                string fullBreadCrumb = context.Get<string>("test.BreadCrumb");
                Assert.Equal("abc", fullBreadCrumb);
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task NoStagesSpecified_AllRunTogether(string serverName)
        {
            int port = RunWebServer(
                serverName,
                NoStagesSpecified);

            return SendRequestAsync(port);
        }

        public void BadStagesSpecified(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use<BreadCrumbMiddleware>("a", "PreHandlerExecute");
            AddStageMarker(app, "Bad");
            app.Use<BreadCrumbMiddleware>("b", "PreHandlerExecute");
            AddStageMarker(app, "Unknown");
            app.Use<BreadCrumbMiddleware>("c", "PreHandlerExecute");
            AddStageMarker(app, "random 13169asg635rs4g3rg3");
            app.Use(context =>
            {
                string fullBreadCrumb = context.Get<string>("test.BreadCrumb");
                Assert.Equal("abc", fullBreadCrumb);
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task BadStagesSpecified_AllRunTogether(string serverName)
        {
            int port = RunWebServer(
                serverName,
                BadStagesSpecified);

            return SendRequestAsync(port);
        }

        public void KnownStagesSpecified(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.Use<BreadCrumbMiddleware>("b", "PostAuthenticate");
            AddStageMarker(app, "PostAuthenticate");
            app.Use<BreadCrumbMiddleware>("c", "Authorize");
            AddStageMarker(app, "Authorize");
            app.Use<BreadCrumbMiddleware>("d", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize");
            app.Use<BreadCrumbMiddleware>("e", "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            app.Use<BreadCrumbMiddleware>("f", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.Use<BreadCrumbMiddleware>("g", "MapHandler");
            AddStageMarker(app, "MapHandler");
            app.Use<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler");
            app.Use<BreadCrumbMiddleware>("i", "AcquireState");
            AddStageMarker(app, "AcquireState");
            app.Use<BreadCrumbMiddleware>("j", "PostAcquireState");
            AddStageMarker(app, "PostAcquireState");
            app.Use<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            AddStageMarker(app, "PreHandlerExecute");
            app.Use(context =>
            {
                string fullBreadCrumb = context.Get<string>("test.BreadCrumb");
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task KnownStagesSpecified_AllRunAtStages(string serverName)
        {
            int port = RunWebServer(
                serverName,
                KnownStagesSpecified);

            return SendRequestAsync(port);
        }

        public void SameStageSpecifiedMultipleTimes(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.Use<BreadCrumbMiddleware>("b", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.Use<BreadCrumbMiddleware>("c", "Authorize");
            AddStageMarker(app, "Authorize");
            app.Use<BreadCrumbMiddleware>("d", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize");
            app.Use<BreadCrumbMiddleware>("e", "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            app.Use<BreadCrumbMiddleware>("f", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.Use<BreadCrumbMiddleware>("g", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.Use<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler");
            app.Use<BreadCrumbMiddleware>("i", "AcquireState");
            AddStageMarker(app, "AcquireState");
            app.Use<BreadCrumbMiddleware>("j", "AcquireState");
            AddStageMarker(app, "AcquireState");
            app.Use<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            AddStageMarker(app, "PreHandlerExecute");
            app.Use(context =>
            {
                string fullBreadCrumb = context.Get<string>("test.BreadCrumb");
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SameStageSpecifiedMultipleTimes_AllRunAtExpectedStages(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SameStageSpecifiedMultipleTimes);

            return SendRequestAsync(port);
        }

        public void NoFinalStageSpecified(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.Use<BreadCrumbMiddleware>("b", "PostAuthenticate");
            AddStageMarker(app, "PostAuthenticate");
            app.Use<BreadCrumbMiddleware>("c", "Authorize");
            AddStageMarker(app, "Authorize");
            app.Use<BreadCrumbMiddleware>("d", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize");
            app.Use<BreadCrumbMiddleware>("e", "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            app.Use<BreadCrumbMiddleware>("f", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.Use<BreadCrumbMiddleware>("g", "MapHandler");
            AddStageMarker(app, "MapHandler");
            app.Use<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler");
            app.Use<BreadCrumbMiddleware>("i", "PreHandlerExecute");
            app.Use<BreadCrumbMiddleware>("j", "PreHandlerExecute");
            app.Use<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            app.Use(context =>
            {
                string fullBreadCrumb = context.Get<string>("test.BreadCrumb");
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task NoFinalStageSpecified_RemaingRunAtPreHandlerExecute(string serverName)
        {
            int port = RunWebServer(
                serverName,
                NoFinalStageSpecified);

            return SendRequestAsync(port);
        }

        public void OutOfOrderMarkers(IAppBuilder app)
        {
            app.UseErrorPage();
            app.Use<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "PostResolveCache"); // 5
            app.Use<BreadCrumbMiddleware>("b", "Authenticate");
            AddStageMarker(app, "PostAuthenticate"); // 1
            app.Use<BreadCrumbMiddleware>("c", "Authenticate");
            AddStageMarker(app, "Authenticate"); // 0
            app.Use<BreadCrumbMiddleware>("d", "Authorize");
            AddStageMarker(app, "Authorize"); // 2
            app.Use<BreadCrumbMiddleware>("e", "PostAuthorize");
            AddStageMarker(app, "ResolveCache"); // 4
            app.Use<BreadCrumbMiddleware>("f", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize"); // 3
            app.Use<BreadCrumbMiddleware>("g", "MapHandler");
            AddStageMarker(app, "MapHandler"); // 6
            app.Use<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostAcquireState"); // 9
            app.Use<BreadCrumbMiddleware>("i", "PostMapHandler");
            AddStageMarker(app, "AcquireState"); // 8
            app.Use<BreadCrumbMiddleware>("j", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler"); // 7
            app.Use<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            AddStageMarker(app, "PreHandlerExecute"); // 10
            app.Use(context =>
            {
                string fullBreadCrumb = context.Get<string>("test.BreadCrumb");
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task OutOfOrderMarkers_AllRunInOrder(string serverName)
        {
            int port = RunWebServer(
                serverName,
                OutOfOrderMarkers);

            return SendRequestAsync(port);
        }

        private Task SendRequestAsync(int port)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port)
                .Then(response => 
                {
                    Assert.Equal(String.Empty, response.Content.ReadAsStringAsync().Result);
                    response.EnsureSuccessStatusCode();
                });
        }

        private void AddStageMarker(IAppBuilder app, string stageName)
        {
            object obj;
            if (app.Properties.TryGetValue("integratedpipeline.StageMarker", out obj))
            {
                Action<IAppBuilder, string> addMarker = (Action<IAppBuilder, string>)obj;
                addMarker(app, stageName);
            }
        }

        private class BreadCrumbMiddleware : OwinMiddleware
        {
            private readonly string _crumb;
            private readonly string _expectedStage;

            public BreadCrumbMiddleware(OwinMiddleware next, string crumb, string expectedStage)
                : base(next)
            {
                _crumb = crumb;
                _expectedStage = expectedStage;
            }

            public override Task Invoke(IOwinContext context)
            {
                string stage = context.Get<string>("integratedpipeline.CurrentStage");
                if (stage != null)
                {
                    Assert.Equal(_expectedStage, stage);
                }

                string fullBreadCrumb = context.Get<string>("test.BreadCrumb");
                fullBreadCrumb += _crumb;
                context.Set("test.BreadCrumb", fullBreadCrumb);
                return Next.Invoke(context);
            }
        }
    }
}
