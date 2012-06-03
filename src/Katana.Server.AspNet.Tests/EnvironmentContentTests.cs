using System;
using System.Threading.Tasks;
using System.Web.Routing;
using Shouldly;
using Xunit;

namespace Katana.Server.AspNet.Tests
{
    public class EnvironmentContentTests : TestsBase
    {
        [Fact]
        public Task ItShouldContainRequestContextWhenCalledThroughRoute()
        {
            var routes = new RouteCollection();
            routes.AddOwinRoute("", WasCalledApp);
            var requestContext = NewRequestContext(routes, NewHttpContext(new Uri("http://localhost")));

            var task = ExecuteRequestContext(requestContext);
            return task.ContinueWith(
                _ =>
                {
                    WasCalled.ShouldBe(true);
                    WasCalledEnvironment.ShouldContainKeyAndValue(typeof(RequestContext).FullName, requestContext);
                });
        }
    }
}
