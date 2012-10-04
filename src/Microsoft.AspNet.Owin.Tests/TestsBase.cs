//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using FakeN.Web;
using Microsoft.AspNet.Owin.Tests.FakeN;
using Owin;

namespace Microsoft.AspNet.Owin.Tests
{
    public class TestsBase
    {
        protected bool WasCalled { get; set; }

        protected IDictionary<string, object> WasCalledInput { get; set; }

        protected Task WasCalledApp(IDictionary<string, object> env)
        {
            WasCalled = true;
            WasCalledInput = env;
            return TaskHelpers.Completed();
        }
        
        protected FakeHttpContext NewHttpContext(Uri url, string method = "GET")
        {
            return new FakeHttpContext(new FakeHttpRequestEx(url, method), new FakeHttpResponseEx());
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
            var httpHandler = (OwinHttpHandler)requestContext.RouteData.RouteHandler.GetHttpHandler(requestContext);
            var task = Task.Factory.FromAsync(httpHandler.BeginProcessRequest, httpHandler.EndProcessRequest,
                                              requestContext.HttpContext, null);
            return task;
        }
    }
}