// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web;
using System.Web.Routing;
using FakeN.Web;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Host.SystemWeb.Tests
{
    public class OwinRouteHandlerTests : TestsBase
    {
        [Fact]
        public void ItShouldReturnAnOwinHttpHandler()
        {
            FakeHttpContext httpContext = NewHttpContext(new Uri("http://localhost"));
            RequestContext requestContext = NewRequestContext(new OwinRoute(string.Empty, () => null), httpContext);

            IHttpHandler httpHandler = requestContext.RouteData.RouteHandler.GetHttpHandler(requestContext);

            requestContext.RouteData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
            httpHandler.ShouldNotBe(null);
            httpHandler.ShouldBeTypeOf<OwinHttpHandler>();
        }
    }
}
