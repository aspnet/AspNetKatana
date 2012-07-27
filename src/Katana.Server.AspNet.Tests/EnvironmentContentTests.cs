using System;
using System.Diagnostics;
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
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.Environment.ShouldContainKeyAndValue(typeof(RequestContext).FullName, requestContext);
                    WasCalledInput.Environment.ShouldContainKey(typeof(HttpContextBase).FullName);
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
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.Environment.ShouldContainKey("owin.RequestMethod");
                    WasCalledInput.Environment.ShouldContainKey("owin.RequestPath");
                    WasCalledInput.Environment.ShouldContainKey("owin.RequestPathBase");
                    WasCalledInput.Environment.ShouldContainKey("owin.RequestQueryString");
                    WasCalledInput.Environment.ShouldContainKey("owin.RequestScheme");
                    WasCalledInput.Environment.ShouldContainKey("owin.Version");
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
                _ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.Environment.ShouldContainKeyAndValue("owin.RequestMethod", "DELTA");
                });
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
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.Environment.ShouldContainKeyAndValue("owin.RequestPathBase", "");
                    WasCalledInput.Environment.ShouldContainKeyAndValue("owin.RequestPath", "/alpha/beta");
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
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                    WasCalledInput.Environment.ShouldContainKeyAndValue("owin.RequestQueryString", "gamma=delta&omega=%2fepsilon");
                });
        }
    }
}
