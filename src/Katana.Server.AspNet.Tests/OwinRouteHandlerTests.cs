using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using FakeN.Web;
using Shouldly;
using Xunit;

namespace Katana.Server.AspNet.Tests
{
    public class OwinRouteHandlerTests : TestsBase
    {
        [Fact]
        public void ItShouldReturnAnOwinHttpHandler()
        {
            var httpContext = NewHttpContext(new Uri("http://localhost"));
            var requestContext = NewRequestContext(new OwinRoute("", () => null), httpContext);

            var httpHandler = requestContext.RouteData.RouteHandler.GetHttpHandler(requestContext);

            requestContext.RouteData.RouteHandler.ShouldBeTypeOf<OwinRouteHandler>();
            httpHandler.ShouldNotBe(null);
            httpHandler.ShouldBeTypeOf<OwinHttpHandler>();
        }

    }
}
