//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using FakeN.Web;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Owin.Tests
{
    public class OwinRouteHandlerTests : TestsBase
    {
        [Fact]
        public void ItShouldReturnAnOwinHttpHandler()
        {
            var httpContext = NewHttpContext(new Uri("http://localhost"));
            var requestContext = NewRequestContext(new OwinRoute(string.Empty, () => null), httpContext);

            var httpHandler = requestContext.RouteData.RouteHandler.GetHttpHandler(requestContext);

            requestContext.RouteData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
            httpHandler.ShouldNotBe(null);
            httpHandler.ShouldBeTypeOf<OwinHttpHandler>();
        }
    }
}
