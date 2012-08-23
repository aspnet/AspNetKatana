using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Owin.Tests
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    public class EnvironmentContentTests : TestsBase
    {
        [Fact]
        public Task ItShouldContainRequestContextAndAnHttpContextBaseWhenCalledThroughRoute()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute<AppDelegate>("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue(typeof(RequestContext).FullName, requestContext);
                    WasCalledInput.ShouldContainKey(typeof(HttpContextBase).FullName);
                });
        }

        [Fact]
        public Task ItShouldContainAllOwinStandardKeys()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute<AppDelegate>("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKey("owin.RequestMethod");
                    WasCalledInput.ShouldContainKey("owin.RequestPath");
                    WasCalledInput.ShouldContainKey("owin.RequestPathBase");
                    WasCalledInput.ShouldContainKey("owin.RequestQueryString");
                    WasCalledInput.ShouldContainKey("owin.RequestScheme");
                    WasCalledInput.ShouldContainKey("owin.Version");
                });
        }

        [Fact]
        public Task ItShouldContainGivenRequestMethod()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute<AppDelegate>("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost"), "DELTA"));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestMethod", "DELTA");
                });
        }

        [Fact]
        public Task ItShouldHaveEmptyPathBaseAndAbsolutePath()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute<AppDelegate>("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost/alpha/beta")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestPathBase", "");
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestPath", "/alpha/beta");
                });
        }

        [Fact]
        public Task ItShouldHaveUnparsedAndEscapedQueryString()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute<AppDelegate>("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost/alpha/beta?gamma=delta&omega=%2fepsilon")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.ShouldContainKeyAndValue("owin.RequestQueryString", "gamma=delta&omega=%2fepsilon");
                });
        }
    }
}
