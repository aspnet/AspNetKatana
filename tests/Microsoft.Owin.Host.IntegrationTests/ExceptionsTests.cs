
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else
namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExceptionsTests : TestBase
    {
        public void UnhandledSyncException(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                throw new Exception();
            }));
        }

        public void UnhandledAsyncException(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.TrySetException(new Exception());
                return tcs.Task;
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SyncException_StatusCode500Expected(string serverName)
        {
            var port = RunWebServer(
                serverName,
                typeof(ExceptionsTests).FullName + ".UnhandledSyncException");

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task AsyncException_StatusCode500Expected(string serverName)
        {
            var port = RunWebServer(
                serverName,
                typeof(ExceptionsTests).FullName + ".UnhandledAsyncException");

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
        }
    }
}
