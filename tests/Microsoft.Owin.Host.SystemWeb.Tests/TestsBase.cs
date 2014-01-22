// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Routing;
using FakeN.Web;
using Microsoft.Owin.Host.SystemWeb.Tests.FakeN;

namespace Microsoft.Owin.Host.SystemWeb.Tests
{
    public class TestsBase
    {
        protected bool WasCalled { get; set; }

        protected IDictionary<string, object> WasCalledInput { get; set; }

        protected Task WasCalledApp(IDictionary<string, object> env)
        {
            WasCalled = true;
            WasCalledInput = env;
            return Utils.CompletedTask;
        }

        protected FakeHttpContext NewHttpContext(Uri url, string method = "GET")
        {
            return new FakeHttpContextEx(new FakeHttpRequestEx(url, method), new FakeHttpResponseEx());
        }

        protected RequestContext NewRequestContext(RouteBase route, FakeHttpContext httpContext)
        {
            RouteData routeData = route.GetRouteData(httpContext);
            return routeData != null ? new RequestContext(httpContext, routeData) : null;
        }

        protected RequestContext NewRequestContext(RouteCollection routes, FakeHttpContext httpContext)
        {
            RouteData routeData = routes.GetRouteData(httpContext);
            return routeData != null ? new RequestContext(httpContext, routeData) : null;
        }

        protected Task ExecuteRequestContext(RequestContext requestContext)
        {
            var httpHandler = (OwinHttpHandler)requestContext.RouteData.RouteHandler.GetHttpHandler(requestContext);
            Task task = Task.Factory.FromAsync(httpHandler.BeginProcessRequest, httpHandler.EndProcessRequest,
                requestContext.HttpContext, null);
            return task;
        }
    }
}
