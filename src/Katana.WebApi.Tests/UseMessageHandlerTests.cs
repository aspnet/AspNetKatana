using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Gate.Builder;
using Owin;
using Shouldly;
using Xunit;

namespace Katana.WebApi.Tests
{
    public class UseMessageHandlerTests
    {
        [Fact]
        public void MessageHandlerWillBeCreatedByAppBuilder()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<HelloWorldHandler>();

            var app = builder.Materialize<HttpMessageHandler>();


            app.ShouldBeTypeOf<HelloWorldHandler>();
            var handler = (HelloWorldHandler)app;
            handler.CtorTwoCalled.ShouldBe(true);
        }

        [Fact]
        public Task CallingAppDelegateShouldInvokeMessageHandler()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<HelloWorldHandler>();

            var app = builder.Materialize();

            var env = new Dictionary<string, object>
                          {
                              {"owin.Version", "1.0"},
                              {"owin.RequestMethod", "GET"},
                              {"owin.RequestScheme", "http"},
                              {"owin.RequestPathBase", ""},
                              {"owin.RequestPath", "/"},
                              {"owin.RequestQueryString", ""},
                              {"owin.RequestHeaders", new Dictionary<string, string[]>()},
                              {"owin.RequestBody", new BodyDelegate((write, end, cancel) => end(null))},
                          };

            var tcs = new TaskCompletionSource<object>();
            app.Invoke(
                env,
                (status, headers, body) =>
                {
                    try
                    {
                        status.ShouldBe("200 OK");
                        headers["Content-Type"].ShouldBe(new[] { "text/plain; charset=utf-8" });
                        tcs.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                },
                tcs.SetException);
            return tcs.Task;
        }
    }
}
