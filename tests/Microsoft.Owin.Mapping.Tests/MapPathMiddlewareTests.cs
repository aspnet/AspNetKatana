// <copyright file="MapPathMiddlewareTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Mapping.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using MsAppFunc = Func<IOwinContext, Task>;

    public class MapPathMiddlewareTests
    {
        private static readonly AppFunc AppFuncNotImplemented = new AppFunc(_ => { throw new NotImplementedException(); });
        private static readonly MsAppFunc FuncNotImplemented = new MsAppFunc(_ => { throw new NotImplementedException(); });
        private static readonly Action<IAppBuilder> ActionNotImplemented = new Action<IAppBuilder>(_ => { throw new NotImplementedException(); });

        private static readonly MsAppFunc Success = new MsAppFunc(context =>
        {
            context.Response.StatusCode = 200;
            return TaskHelpers.FromResult<object>(null);
        });

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            var builder = new AppBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.MapPath(null, FuncNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPath("/foo", (AppFunc)null));
            Assert.Throws<ArgumentNullException>(() => builder.MapPath(null, ActionNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPath("/foo", (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapPathMiddleware(null, AppFuncNotImplemented, "/foo"));
            Assert.Throws<ArgumentNullException>(() => new MapPathMiddleware(AppFuncNotImplemented, null, "/foo"));
            Assert.Throws<ArgumentNullException>(() => new MapPathMiddleware(AppFuncNotImplemented, AppFuncNotImplemented, null));
        }

        [Theory]
        [InlineData("/foo", "", "/foo")]
        [InlineData("/foo", "", "/foo/")]
        [InlineData("/foo", "/Bar", "/foo")]
        [InlineData("/foo", "/Bar", "/foo/cho")]
        [InlineData("/foo", "/Bar", "/foo/cho/")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho/do")]
        public void PathMatchFunc_BranchTaken(string matchPath, string basePath, string requestPath)
        {
            IOwinContext context = CreateRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, Success);
            var app = builder.Build<MsAppFunc>();
            app(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath + matchPath, context.Request.PathBase);
            Assert.Equal(requestPath.Substring(matchPath.Length), context.Request.Path);
        }

        [Theory]
        [InlineData("/foo", "", "/foo")]
        [InlineData("/foo", "", "/foo/")]
        [InlineData("/foo", "/Bar", "/foo")]
        [InlineData("/foo", "/Bar", "/foo/cho")]
        [InlineData("/foo", "/Bar", "/foo/cho/")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho")]
        [InlineData("/foo/cho", "/Bar", "/foo/cho/do")]
        public void PathMatchAction_BranchTaken(string matchPath, string basePath, string requestPath)
        {
            IOwinContext context = CreateRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, subBuilder => subBuilder.UseApp(Success));
            var app = builder.Build<MsAppFunc>();
            app(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath + matchPath, context.Request.PathBase);
            Assert.Equal(requestPath.Substring(matchPath.Length), context.Request.Path);
        }

        [Theory]
        [InlineData("/foo/", "", "/foo")]
        [InlineData("/foo/", "", "/foo/")]
        [InlineData("/foo/", "/Bar", "/foo")]
        [InlineData("/foo/", "/Bar", "/foo/cho")]
        [InlineData("/foo/", "/Bar", "/foo/cho/")]
        [InlineData("/foo/cho/", "/Bar", "/foo/cho")]
        [InlineData("/foo/cho/", "/Bar", "/foo/cho/do")]
        public void MatchPathHasTrailingSlash_Trimmed(string matchPath, string basePath, string requestPath)
        {
            IOwinContext context = CreateRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, Success);
            var app = builder.Build<MsAppFunc>();
            app(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath + matchPath.Substring(0, matchPath.Length - 1), context.Request.PathBase);
            Assert.Equal(requestPath.Substring(matchPath.Length - 1), context.Request.Path);
        }

        [Theory]
        [InlineData("/foo", "", "")]
        [InlineData("/foo", "/bar", "")]
        [InlineData("/foo", "", "/bar")]
        [InlineData("/foo", "/foo", "")]
        [InlineData("/foo", "/foo", "/bar")]
        [InlineData("/foo", "", "/bar/foo")]
        [InlineData("/foo/bar", "/foo", "/bar")]
        public void PathMismatchFunc_PassedThrough(string matchPath, string basePath, string requestPath)
        {
            IOwinContext context = CreateRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, FuncNotImplemented);
            builder.UseApp(Success);
            var app = builder.Build<MsAppFunc>();
            app(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase);
            Assert.Equal(requestPath, context.Request.Path);
        }

        [Theory]
        [InlineData("/foo", "", "")]
        [InlineData("/foo", "/bar", "")]
        [InlineData("/foo", "", "/bar")]
        [InlineData("/foo", "/foo", "")]
        [InlineData("/foo", "/foo", "/bar")]
        [InlineData("/foo", "", "/bar/foo")]
        [InlineData("/foo/bar", "/foo", "/bar")]
        public void PathMismatchAction_PassedThrough(string matchPath, string basePath, string requestPath)
        {
            IOwinContext context = CreateRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, subBuilder => subBuilder.UseApp(FuncNotImplemented));
            builder.UseApp(Success);
            var app = builder.Build<MsAppFunc>();
            app(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase);
            Assert.Equal(requestPath, context.Request.Path);
        }

        [Fact]
        public void ChainedRoutes_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapPath("/route1", subBuilder =>
            {
                subBuilder.MapPath("/subroute1", Success);
                subBuilder.UseApp(FuncNotImplemented);
            });
            builder.MapPath("/route2/subroute2", Success);
            var app = builder.Build<MsAppFunc>();

            IOwinContext context = CreateRequest(string.Empty, "/route1");
            Assert.Throws<NotImplementedException>(() => app(context));

            context = CreateRequest(string.Empty, "/route1/subroute1");
            app(context);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("/route1/subroute1", context.Request.PathBase);
            Assert.Equal(string.Empty, context.Request.Path);

            context = CreateRequest(string.Empty, "/route2");
            app(context);
            Assert.Equal(404, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase);
            Assert.Equal("/route2", context.Request.Path);

            context = CreateRequest(string.Empty, "/route2/subroute2");
            app(context);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("/route2/subroute2", context.Request.PathBase);
            Assert.Equal(string.Empty, context.Request.Path);

            context = CreateRequest(string.Empty, "/route2/subroute2/subsub2");
            app(context);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal("/route2/subroute2", context.Request.PathBase);
            Assert.Equal("/subsub2", context.Request.Path);
        }

        private IOwinContext CreateRequest(string basePath, string requestPath)
        {
            IOwinContext context = new OwinContext();
            context.Request.PathBase = basePath;
            context.Request.Path = requestPath;
            return context;
        }
    }
}
