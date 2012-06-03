using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;

namespace Katana.Server.AspNet.Tests
{
    public class OwinHttpHandlerTests : TestsBase
    {
        private bool _wasCalled;

        [Fact]
        public void ProcessRequestIsNotImplemeted()
        {
            var httpHandler = new OwinHttpHandler();
            var httpContext = NewHttpContext(new Uri("http://localhost"));

            Should.Throw<NotImplementedException>(() => httpHandler.ProcessRequest(httpContext));
        }

        [Fact]
        public Task ItShouldCallAppDelegateWhenBeginProcessRequestCalled()
        {
            var httpHandler = new OwinHttpHandler(WasCalledApp);
            var httpContext = NewHttpContext(new Uri("http://localhost"));

            var task = Task.Factory.FromAsync(httpHandler.BeginProcessRequest, httpHandler.EndProcessRequest, httpContext, null);
            return task.ContinueWith(_ => _wasCalled.ShouldBe(true));
        }


        private void WasCalledApp(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            _wasCalled = true;
            result("200 OK", new Dictionary<string, IEnumerable<string>>(), (write, flush, end, cancel) => end(null));
        }


    }
}
