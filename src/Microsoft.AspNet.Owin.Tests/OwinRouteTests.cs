//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Routing;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Owin.Tests
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    public class OwinRouteTests : TestsBase
    {
        [Fact]
        public void OwinRouteShouldReturnNullWhenRequestDoesNotStartWithGivenPath()
        {
            var route = new OwinRoute("alpha", null);
            var httpContext = NewHttpContext(new Uri("http://localhost/beta"));

            var routeData = route.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void OwinRouteShouldProvideRouteDataWithAnOwinRouteHandlerWhenRequestStartsWithGivenPath()
        {
            var route = new OwinRoute("alpha", null);
            var httpContext = NewHttpContext(new Uri("http://localhost/alpha"));

            var routeData = route.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public void AddOwinRouteOnRouteCollectionShouldReturnNullForMismatchedPaths()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("alpha");
            var httpContext = NewHttpContext(new Uri("http://localhost/beta"));

            var routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void AddOwinRouteOnRouteCollectionShouldMatchGivenPath()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("alpha");
            var httpContext = NewHttpContext(new Uri("http://localhost/alpha"));

            var routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public void ItShouldMatchLongerRequestPaths()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("alpha");
            var httpContext = NewHttpContext(new Uri("http://localhost/alpha-longer"));

            var routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public void ItShouldNotMatchShorterRequestPaths()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("alpha");
            var httpContext = NewHttpContext(new Uri("http://localhost/alph"));

            var routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void ItShouldNotMatchWhenTrailingSlashIsAbsent()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("alpha/");
            var httpContext = NewHttpContext(new Uri("http://localhost/alpha"));

            var routeData = routes.GetRouteData(httpContext);

            routeData.ShouldBe(null);
        }

        [Fact]
        public void ItShouldMatchWhenTrailingSlashIsPresent()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("alpha/");
            var httpContext = NewHttpContext(new Uri("http://localhost/alpha/"));

            var routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public void QueryStringShouldNotAffectMatch()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("alpha");
            var httpContext = NewHttpContext(new Uri("http://localhost/alpha?gamma=delta"));

            var routeData = routes.GetRouteData(httpContext);

            routeData.ShouldNotBe(null);
            routeData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
        }

        [Fact]
        public Task AppDelegateAccessorPassesFromOwinRouteThroughToOwinHttpHandler()
        {
            var route = new OwinRoute(string.Empty, () => WasCalledApp);
            var httpContext = NewHttpContext(new Uri("http://localhost"));
            var requestContext = NewRequestContext(route, httpContext);

            var task = ExecuteRequestContext(requestContext);
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
            routes.MapOwinRoute<AppDelegate>(string.Empty, WasCalledApp);
            var httpContext = NewHttpContext(new Uri("http://localhost"));
            var requestContext = NewRequestContext(routes, httpContext);

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(_ =>
            {
                task.Exception.ShouldBe(null);
                WasCalled.ShouldBe(true);
            });
        }
    }
}
