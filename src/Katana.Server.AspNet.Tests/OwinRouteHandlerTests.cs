using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using Shouldly;
using Xunit;

namespace Katana.Server.AspNet.Tests
{
    public class OwinRouteHandlerTests : TestsBase
    {
        [Fact]
        public void ItShouldReturnAnOwinHttpHandler()
        {
            var routeHandler = new OwinRouteHandler();
            var httpContext = NewHttpContext(new Uri("http://localhost"));
            var routeData = new RouteData(new OwinRoute(""), routeHandler);
            var requestContext = new RequestContext(httpContext, routeData);

            var httpHandler = routeHandler.GetHttpHandler(requestContext);

            httpHandler.ShouldNotBe(null);
            httpHandler.ShouldBeTypeOf<OwinHttpHandler>();
        }
    }
}
