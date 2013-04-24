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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
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
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class IntegratedPipelineTests : TestBase
    {
        public void NoStagesSpecified(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseType<BreadCrumbMiddleware>("a", "PreHandlerExecute");
            app.UseType<BreadCrumbMiddleware>("b", "PreHandlerExecute");
            app.UseType<BreadCrumbMiddleware>("c", "PreHandlerExecute");
            app.Run((AppFunc)(environment =>
            {
                string fullBreadCrumb = environment.Get<string>("test.BreadCrumb", string.Empty);
                Assert.Equal("abc", fullBreadCrumb);
                return TaskHelpers.Completed();
            }));
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
            app.UseType<BreadCrumbMiddleware>("a", "PreHandlerExecute");
            AddStageMarker(app, "Bad");
            app.UseType<BreadCrumbMiddleware>("b", "PreHandlerExecute");
            AddStageMarker(app, "Unknown");
            app.UseType<BreadCrumbMiddleware>("c", "PreHandlerExecute");
            AddStageMarker(app, "random 13169asg635rs4g3rg3");
            app.Run((AppFunc)(environment =>
            {
                string fullBreadCrumb = environment.Get<string>("test.BreadCrumb", string.Empty);
                Assert.Equal("abc", fullBreadCrumb);
                return TaskHelpers.Completed();
            }));
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
            app.UseType<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.UseType<BreadCrumbMiddleware>("b", "PostAuthenticate");
            AddStageMarker(app, "PostAuthenticate");
            app.UseType<BreadCrumbMiddleware>("c", "Authorize");
            AddStageMarker(app, "Authorize");
            app.UseType<BreadCrumbMiddleware>("d", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize");
            app.UseType<BreadCrumbMiddleware>("e", "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            app.UseType<BreadCrumbMiddleware>("f", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.UseType<BreadCrumbMiddleware>("g", "MapHandler");
            AddStageMarker(app, "MapHandler");
            app.UseType<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler");
            app.UseType<BreadCrumbMiddleware>("i", "AcquireState");
            AddStageMarker(app, "AcquireState");
            app.UseType<BreadCrumbMiddleware>("j", "PostAcquireState");
            AddStageMarker(app, "PostAcquireState");
            app.UseType<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            AddStageMarker(app, "PreHandlerExecute");
            app.Run((AppFunc)(environment =>
            {
                string fullBreadCrumb = environment.Get<string>("test.BreadCrumb", string.Empty);
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            }));
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
            app.UseType<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.UseType<BreadCrumbMiddleware>("b", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.UseType<BreadCrumbMiddleware>("c", "Authorize");
            AddStageMarker(app, "Authorize");
            app.UseType<BreadCrumbMiddleware>("d", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize");
            app.UseType<BreadCrumbMiddleware>("e", "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            app.UseType<BreadCrumbMiddleware>("f", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.UseType<BreadCrumbMiddleware>("g", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.UseType<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler");
            app.UseType<BreadCrumbMiddleware>("i", "AcquireState");
            AddStageMarker(app, "AcquireState");
            app.UseType<BreadCrumbMiddleware>("j", "AcquireState");
            AddStageMarker(app, "AcquireState");
            app.UseType<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            AddStageMarker(app, "PreHandlerExecute");
            app.Run((AppFunc)(environment =>
            {
                string fullBreadCrumb = environment.Get<string>("test.BreadCrumb", string.Empty);
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            }));
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
            app.UseType<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "Authenticate");
            app.UseType<BreadCrumbMiddleware>("b", "PostAuthenticate");
            AddStageMarker(app, "PostAuthenticate");
            app.UseType<BreadCrumbMiddleware>("c", "Authorize");
            AddStageMarker(app, "Authorize");
            app.UseType<BreadCrumbMiddleware>("d", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize");
            app.UseType<BreadCrumbMiddleware>("e", "ResolveCache");
            AddStageMarker(app, "ResolveCache");
            app.UseType<BreadCrumbMiddleware>("f", "PostResolveCache");
            AddStageMarker(app, "PostResolveCache");
            app.UseType<BreadCrumbMiddleware>("g", "MapHandler");
            AddStageMarker(app, "MapHandler");
            app.UseType<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler");
            app.UseType<BreadCrumbMiddleware>("i", "PreHandlerExecute");
            app.UseType<BreadCrumbMiddleware>("j", "PreHandlerExecute");
            app.UseType<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            app.Run((AppFunc)(environment =>
            {
                string fullBreadCrumb = environment.Get<string>("test.BreadCrumb", string.Empty);
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            }));
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
            app.UseType<BreadCrumbMiddleware>("a", "Authenticate");
            AddStageMarker(app, "PostResolveCache"); // 5
            app.UseType<BreadCrumbMiddleware>("b", "Authenticate");
            AddStageMarker(app, "PostAuthenticate"); // 1
            app.UseType<BreadCrumbMiddleware>("c", "Authenticate");
            AddStageMarker(app, "Authenticate"); // 0
            app.UseType<BreadCrumbMiddleware>("d", "Authorize");
            AddStageMarker(app, "Authorize"); // 2
            app.UseType<BreadCrumbMiddleware>("e", "PostAuthorize");
            AddStageMarker(app, "ResolveCache"); // 4
            app.UseType<BreadCrumbMiddleware>("f", "PostAuthorize");
            AddStageMarker(app, "PostAuthorize"); // 3
            app.UseType<BreadCrumbMiddleware>("g", "MapHandler");
            AddStageMarker(app, "MapHandler"); // 6
            app.UseType<BreadCrumbMiddleware>("h", "PostMapHandler");
            AddStageMarker(app, "PostAcquireState"); // 9
            app.UseType<BreadCrumbMiddleware>("i", "PostMapHandler");
            AddStageMarker(app, "AcquireState"); // 8
            app.UseType<BreadCrumbMiddleware>("j", "PostMapHandler");
            AddStageMarker(app, "PostMapHandler"); // 7
            app.UseType<BreadCrumbMiddleware>("k", "PreHandlerExecute");
            AddStageMarker(app, "PreHandlerExecute"); // 10
            app.Run((AppFunc)(environment =>
            {
                string fullBreadCrumb = environment.Get<string>("test.BreadCrumb", string.Empty);
                Assert.Equal("abcdefghijk", fullBreadCrumb);
                return TaskHelpers.Completed();
            }));
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

        private class BreadCrumbMiddleware
        {
            private AppFunc _next;
            private string _crumb;
            private string _expectedStage;

            public BreadCrumbMiddleware(AppFunc next, string crumb, string expectedStage)
            {
                _next = next;
                _crumb = crumb;
                _expectedStage = expectedStage;
            }

            public Task Invoke(IDictionary<string, object> environment)
            {
                string stage = environment.Get<string>("integratedpipeline.CurrentStage", null);
                if (stage != null)
                {
                    Assert.Equal(_expectedStage, stage);
                }

                string fullBreadCrumb = environment.Get<string>("test.BreadCrumb", string.Empty);
                fullBreadCrumb += _crumb;
                environment["test.BreadCrumb"] = fullBreadCrumb;
                return _next(environment);
            }
        }
    }
}
