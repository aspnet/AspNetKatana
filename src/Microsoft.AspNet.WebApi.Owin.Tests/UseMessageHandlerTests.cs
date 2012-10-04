//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Owin.Builder;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.WebApi.Owin.Tests
{
    public class UseMessageHandlerTests
    {
        [Fact]
        public void MessageHandlerWillBeCreatedByAppBuilder()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<HelloWorldHandler>();

            var app = builder.Build<HttpMessageHandler>();


            app.ShouldBeTypeOf<HelloWorldHandler>();
            var handler = (HelloWorldHandler)app;
            handler.CtorTwoCalled.ShouldBe(true);
        }

        [Fact]
        public Task CallingAppDelegateShouldInvokeMessageHandler()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<HelloWorldHandler>();

            var app = builder.Build<AppDelegate>();

            CallParameters call = new CallParameters();
            call.Environment = new Dictionary<string, object>
                          {
                              {"owin.Version", "1.0"},
                              {"owin.RequestMethod", "GET"},
                              {"owin.RequestScheme", "http"},
                              {"owin.RequestPathBase", ""},
                              {"owin.RequestPath", "/"},
                              {"owin.RequestQueryString", ""},
                          };
            call.Headers = new Dictionary<string, string[]>();

            return app.Invoke(call).Then(
                result =>
                {
                    result.Status.ShouldBe(200);
                    result.Headers["Content-Type"].ShouldBe(new[] { "text/plain; charset=utf-8" });
                });
        }

        [Fact]
        public Task CallingMessageHandlerShouldInvokeAppDelegate()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<PassThroughHandler>();
            builder.UseFunc<AppDelegate>(
                appDelegate =>
                {
                    return appCall =>
                    {
                        appCall.Headers.ShouldNotBe(null);
                        appCall.Environment.ShouldNotBe(null);

                        ResultParameters result = new ResultParameters();
                        result.Status = 200;
                        result.Headers = new Dictionary<string, string[]>() 
                            { {"Content-Type", new string[] { "text/plain; charset=utf-8"} } } ;
                        result.Properties = new Dictionary<string, object>();
                        result.Body = null;
                        return TaskHelpers.FromResult(result);
                    };
                });

            var app = builder.Build<AppDelegate>();

            CallParameters call = new CallParameters();
            call.Environment = new Dictionary<string, object>
                          {
                              {"owin.Version", "1.0"},
                              {"owin.RequestMethod", "GET"},
                              {"owin.RequestScheme", "http"},
                              {"owin.RequestPathBase", ""},
                              {"owin.RequestPath", "/"},
                              {"owin.RequestQueryString", ""},
                          };
            call.Headers = new Dictionary<string, string[]>();

            return app.Invoke(call).Then(
                result =>
                {
                    result.Status.ShouldBe(200);
                    result.Headers["Content-Type"].ShouldBe(new[] { "text/plain; charset=utf-8" });
                });
        }
    }
}
