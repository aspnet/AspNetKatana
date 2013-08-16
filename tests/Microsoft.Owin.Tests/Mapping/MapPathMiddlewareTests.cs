// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Mapping.Tests
{
    public class MapPathMiddlewareTests
    {
        private static readonly Action<IAppBuilder> ActionNotImplemented = new Action<IAppBuilder>(_ => { throw new NotImplementedException(); });

        private static Task Success(IOwinContext context)
        {
            context.Response.StatusCode = 200;
            context.Set("test.PathBase", context.Request.PathBase.Value);
            context.Set("test.Path", context.Request.Path.Value);
            return Task.FromResult<object>(null);
        }

        private static void UseSuccess(IAppBuilder app)
        {
            app.Run(Success);
        }

        private static Task NotImplemented(IOwinContext context)
        {
            throw new NotImplementedException();
        }

        private static void UseNotImplemented(IAppBuilder app)
        {
            app.Run(NotImplemented);
        }

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            var builder = new AppBuilder();
            var noMiddleware = new AppBuilder().Build<OwinMiddleware>();
            var noOptions = new MapOptions();
            Assert.Throws<ArgumentNullException>(() => builder.Map(null, ActionNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.Map("/foo", (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapMiddleware(null, noOptions));
            Assert.Throws<ArgumentNullException>(() => new MapMiddleware(noMiddleware, null));
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
            builder.Map(matchPath, UseSuccess);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase.Value);
            Assert.Equal(requestPath, context.Request.Path.Value);
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
            builder.Map(matchPath, subBuilder => subBuilder.Run(Success));
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath + matchPath, context.Get<string>("test.PathBase"));
            Assert.Equal(requestPath.Substring(matchPath.Length), context.Get<string>("test.Path"));
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/foo/")]
        [InlineData("/foo/cho/")]
        public void MatchPathWithTrailingSlashThrowsException(string matchPath)
        {
            // based on Exception instead of ArgumentException because
            // it's wrapped by a TargetInvocationException as a side-effect of reflection
            Should.Throw<Exception>(() => new AppBuilder().Map(matchPath, map => { }).Build());
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
            builder.Map(matchPath, UseNotImplemented);
            builder.Run(Success);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase.Value);
            Assert.Equal(requestPath, context.Request.Path.Value);
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
            builder.Map(matchPath, UseNotImplemented);
            builder.Run(Success);
            var app = builder.Build<OwinMiddleware>();
            app.Invoke(context);

            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(basePath, context.Request.PathBase.Value);
            Assert.Equal(requestPath, context.Request.Path.Value);
        }

        [Fact]
        public void ChainedRoutes_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.Map("/route1", map =>
            {
                map.Map((string)"/subroute1", UseSuccess);
                map.Run(NotImplemented);
            });
            builder.Map("/route2/subroute2", UseSuccess);
            var app = builder.Build<OwinMiddleware>();

            IOwinContext context = CreateRequest(string.Empty, "/route1");
            Assert.Throws<AggregateException>(() => app.Invoke(context).Wait());

            context = CreateRequest(string.Empty, "/route1/subroute1");
            app.Invoke(context);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route1/subroute1", context.Request.Path.Value);

            context = CreateRequest(string.Empty, "/route2");
            app.Invoke(context);
            Assert.Equal(404, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route2", context.Request.Path.Value);

            context = CreateRequest(string.Empty, "/route2/subroute2");
            app.Invoke(context);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route2/subroute2", context.Request.Path.Value);

            context = CreateRequest(string.Empty, "/route2/subroute2/subsub2");
            app.Invoke(context);
            Assert.Equal(200, context.Response.StatusCode);
            Assert.Equal(string.Empty, context.Request.PathBase.Value);
            Assert.Equal("/route2/subroute2/subsub2", context.Request.Path.Value);
        }

        private IOwinContext CreateRequest(string basePath, string requestPath)
        {
            IOwinContext context = new OwinContext();
            context.Request.PathBase = new PathString(basePath);
            context.Request.Path = new PathString(requestPath);
            return context;
        }
    }
}
