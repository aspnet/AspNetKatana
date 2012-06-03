using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Routing;
using FakeN.Web;
using Katana.Server.AspNet.Tests.FakeN;
using Owin;

namespace Katana.Server.AspNet.Tests
{
    public class TestsBase
    {
        protected bool WasCalled;
        protected IDictionary<string, object> WasCalledEnvironment;

        protected void WasCalledApp(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            WasCalled = true;
            WasCalledEnvironment = env;
            result("200 OK", new Dictionary<string, IEnumerable<string>>(), (write, flush, end, cancel) => end(null));
        }

        protected FakeHttpContext NewHttpContext(Uri url)
        {
            return new FakeHttpContext(new FakeHttpRequestEx(url));
        }

        protected RequestContext NewRequestContext(RouteBase route, FakeHttpContext httpContext)
        {
            var routeData = route.GetRouteData(httpContext);
            return routeData != null ? new RequestContext(httpContext, routeData) : null;
        }

        protected RequestContext NewRequestContext(RouteCollection routes, FakeHttpContext httpContext)
        {
            var routeData = routes.GetRouteData(httpContext);
            return routeData != null ? new RequestContext(httpContext, routeData) : null;
        }

        protected Task ExecuteRequestContext(RequestContext requestContext)
        {
            var httpHandler = (OwinHttpHandler) requestContext.RouteData.RouteHandler.GetHttpHandler(requestContext);
            var task = Task.Factory.FromAsync(httpHandler.BeginProcessRequest, httpHandler.EndProcessRequest,
                                              requestContext.HttpContext, null);
            return task;
        }
    }
}