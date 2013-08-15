// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Routing;
using FakeN.Web;
using Shouldly;
using Xunit;

#if NET40
namespace Microsoft.Owin.Host.SystemWeb.Tests
#else

namespace Microsoft.Owin.Host.SystemWeb.Tests45
#endif
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    public class OwinRouteTests : TestsBase
    {
        [Fact]
        public void OwinRouteShouldReturnNullWhenRequestDoesNotStartWithGivenPath()
        {
            var route = new OwinRoute("alpha", null);
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/beta"));

            RouteData routeData = route.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void OwinRouteShouldProvideRouteDataWithAnOwinRouteHandlerWhenRequestStartsWithGivenPath()
        {
            var route = new OwinRoute("alpha", null);
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alpha"));

            RouteData routeData = route.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public void AddOwinRouteOnRouteCollectionShouldReturnNullForMismatchedPaths()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/beta"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void AddOwinRouteOnRouteCollectionShouldMatchGivenPath()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alpha"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public void ItShouldNotMatchLongerRequestPaths()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alpha-longer"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void ItShouldNotMatchShorterRequestPaths()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alph"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void ItShouldNotMatchWhenTrailingSlashIsAbsent()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha/");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alpha"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void ItShouldMatchWhenTrailingSlashIsPresent()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha/");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alpha/"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public void QueryStringShouldNotAffectMatch()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alpha?gamma=delta"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public Task AppDelegateAccessorPassesFromOwinRouteThroughToOwinHttpHandler()
        {
            var route = new OwinRoute(string.Empty, () => OwinBuilder.Build(WasCalledApp));
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost"));
            RequestContext requestContext = NewRequestContext(route, httpContext);

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(_ =>
            {
                task.Exception.ShouldBe(null);
                WasCalled.ShouldBe(true);
            });
        }

        [Fact]
        public Task AppDelegateAccessorPassesFromRouteCollectionThroughToOwinHttpHandler()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath<AppDelegate>(string.Empty, WasCalledApp);
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost"));
            RequestContext requestContext = NewRequestContext(routes, httpContext);

            Task task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(_ =>
            {
                task.Exception.ShouldBe(null);
                WasCalled.ShouldBe(true);
            });
        }

        [Fact]
        public void ItShouldNotMatchPrefixOfLongerSegment()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alphabeta"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void ItShouldMatchEntireSegment()
        {
            var routes = new RouteCollection();
            routes.MapOwinPath("alpha");
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost/alpha/beta"));

            RouteData routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }
    }
}
