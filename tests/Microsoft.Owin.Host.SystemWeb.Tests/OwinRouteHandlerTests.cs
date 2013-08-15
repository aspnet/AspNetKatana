// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web;
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
