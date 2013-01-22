// <copyright file="MapPathMiddlewareTests.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;
using Owin.Builder;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Mapping.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class MapPathMiddlewareTests
    {
        private static readonly AppFunc FuncNotImplemented = new AppFunc(_ => { throw new NotImplementedException(); });
        private static readonly Action<IAppBuilder> ActionNotImplemented = new Action<IAppBuilder>(_ => { throw new NotImplementedException(); });

        private static readonly AppFunc Success = new AppFunc(environment =>
        {
            environment["owin.ResponseStatusCode"] = 200;
            return null;
        });

        [Fact]
        public void NullArguments_ArgumentNullException()
        {
            var builder = new AppBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.MapPath(null, FuncNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPath("/foo", (AppFunc)null));
            Assert.Throws<ArgumentNullException>(() => builder.MapPath(null, ActionNotImplemented));
            Assert.Throws<ArgumentNullException>(() => builder.MapPath("/foo", (Action<IAppBuilder>)null));
            Assert.Throws<ArgumentNullException>(() => new MapPathMiddleware(null, FuncNotImplemented, "/foo"));
            Assert.Throws<ArgumentNullException>(() => new MapPathMiddleware(FuncNotImplemented, null, "/foo"));
            Assert.Throws<ArgumentNullException>(() => new MapPathMiddleware(FuncNotImplemented, FuncNotImplemented, null));
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
            IDictionary<string, object> environment = CreateEmptyRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, Success);
            var app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal(basePath + matchPath, environment["owin.RequestPathBase"]);
            Assert.Equal(requestPath.Substring(matchPath.Length), environment["owin.RequestPath"]);
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
            IDictionary<string, object> environment = CreateEmptyRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, subBuilder => subBuilder.Run(Success));
            var app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal(basePath + matchPath, environment["owin.RequestPathBase"]);
            Assert.Equal(requestPath.Substring(matchPath.Length), environment["owin.RequestPath"]);
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
            IDictionary<string, object> environment = CreateEmptyRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, Success);
            var app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal(basePath + matchPath.Substring(0, matchPath.Length - 1), environment["owin.RequestPathBase"]);
            Assert.Equal(requestPath.Substring(matchPath.Length - 1), environment["owin.RequestPath"]);
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
            IDictionary<string, object> environment = CreateEmptyRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, FuncNotImplemented);
            builder.Run(Success);
            var app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal(basePath, environment["owin.RequestPathBase"]);
            Assert.Equal(requestPath, environment["owin.RequestPath"]);
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
            IDictionary<string, object> environment = CreateEmptyRequest(basePath, requestPath);
            IAppBuilder builder = new AppBuilder();
            builder.MapPath(matchPath, subBuilder => subBuilder.Run(FuncNotImplemented));
            builder.Run(Success);
            var app = builder.Build<AppFunc>();
            app(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal(basePath, environment["owin.RequestPathBase"]);
            Assert.Equal(requestPath, environment["owin.RequestPath"]);
        }

        [Fact]
        public void ChainedRoutes_Success()
        {
            IAppBuilder builder = new AppBuilder();
            builder.MapPath("/route1", subBuilder =>
            {
                subBuilder.MapPath("/subroute1", Success);
                subBuilder.Run(FuncNotImplemented);
            });
            builder.MapPath("/route2/subroute2", Success);
            var app = builder.Build<AppFunc>();

            IDictionary<string, object> environment = CreateEmptyRequest(string.Empty, "/route1");
            Assert.Throws<NotImplementedException>(() => app(environment));

            environment = CreateEmptyRequest(string.Empty, "/route1/subroute1");
            app(environment);
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal("/route1/subroute1", environment["owin.RequestPathBase"]);
            Assert.Equal(string.Empty, environment["owin.RequestPath"]);

            environment = CreateEmptyRequest(string.Empty, "/route2");
            app(environment);
            Assert.Equal(404, environment["owin.ResponseStatusCode"]);
            Assert.Equal(string.Empty, environment["owin.RequestPathBase"]);
            Assert.Equal("/route2", environment["owin.RequestPath"]);

            environment = CreateEmptyRequest(string.Empty, "/route2/subroute2");
            app(environment);
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal("/route2/subroute2", environment["owin.RequestPathBase"]);
            Assert.Equal(string.Empty, environment["owin.RequestPath"]);

            environment = CreateEmptyRequest(string.Empty, "/route2/subroute2/subsub2");
            app(environment);
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
            Assert.Equal("/route2/subroute2", environment["owin.RequestPathBase"]);
            Assert.Equal("/subsub2", environment["owin.RequestPath"]);
        }

        private IDictionary<string, object> CreateEmptyRequest(string basePath, string requestPath)
        {
            IDictionary<string, object> environment = new Dictionary<string, object>();
            environment["owin.RequestPathBase"] = basePath;
            environment["owin.RequestPath"] = requestPath;
            return environment;
        }
    }
}
