using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Owin.Tests
{
    public class OwinHttpHandlerTests : TestsBase
    {

        [Fact]
        public void ProcessRequestIsNotImplemeted()
        {
            var httpHandler = new OwinHttpHandler("", ()=>null);
            var httpContext = NewHttpContext(new Uri("http://localhost"));

            Should.Throw<NotImplementedException>(() => httpHandler.ProcessRequest(httpContext));
        }

        [Fact]
        public Task ItShouldCallAppDelegateWhenBeginProcessRequestCalled()
        {
            var httpHandler = new OwinHttpHandler("", WasCalledApp);
            var httpContext = NewHttpContext(new Uri("http://localhost"));

            var task = Task.Factory.FromAsync(httpHandler.BeginProcessRequest, httpHandler.EndProcessRequest, httpContext, null);
            return task.ContinueWith(_ =>
                {
                    task.Exception.ShouldBe(null);
                    WasCalled.ShouldBe(true);
                });
        }
    }
}
