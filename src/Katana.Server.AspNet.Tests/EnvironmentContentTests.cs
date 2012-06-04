using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Shouldly;
using Xunit;

namespace Katana.Server.AspNet.Tests
{
    public class EnvironmentContentTests : TestsBase
    {
        [Fact]
        public Task ItShouldContainRequestContextAndAnHttpContextBaseWhenCalledThroughRoute()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    WasCalled.ShouldBe(true);
                    WasCalledEnvironment.ShouldContainKeyAndValue(typeof(RequestContext).FullName, requestContext);
                    WasCalledEnvironment.ShouldContainKey(typeof(HttpContextBase).FullName);
                });
        }

        [Fact]
        public Task ItShouldContainAllOwinStandardKeys()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    WasCalled.ShouldBe(true);
                    WasCalledEnvironment.ShouldContainKey("owin.RequestMethod");
                    WasCalledEnvironment.ShouldContainKey("owin.RequestPath");
                    WasCalledEnvironment.ShouldContainKey("owin.RequestPathBase");
                    WasCalledEnvironment.ShouldContainKey("owin.RequestQueryString");
                    WasCalledEnvironment.ShouldContainKey("owin.RequestHeaders");
                    WasCalledEnvironment.ShouldContainKey("owin.RequestBody");
                    WasCalledEnvironment.ShouldContainKey("owin.RequestScheme");
                    WasCalledEnvironment.ShouldContainKey("owin.Version");
                });
        }

        [Fact]
        public Task ItShouldContainGivenRequestMethod()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost"), "DELTA"));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ => WasCalledEnvironment.ShouldContainKeyAndValue("owin.RequestMethod", "DELTA"));
        }

        [Fact]
        public Task ItShouldHaveEmptyPathBaseAndAbsolutePath()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost/alpha/beta")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    WasCalledEnvironment.ShouldContainKeyAndValue("owin.RequestPathBase", "");
                    WasCalledEnvironment.ShouldContainKeyAndValue("owin.RequestPath", "/alpha/beta");
                });
        }

        [Fact]
        public Task ItShouldHaveUnparsedAndEscapedQueryString()
        {
            var routes = new RouteCollection();
            routes.MapOwinRoute("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost/alpha/beta?gamma=delta&omega=%2fepsilon")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    WasCalledEnvironment.ShouldContainKeyAndValue("owin.RequestQueryString", "gamma=delta&omega=%2fepsilon");
                });
        }
    }
}
