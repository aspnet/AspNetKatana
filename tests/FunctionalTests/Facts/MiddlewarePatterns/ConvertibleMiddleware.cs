// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;
using Xunit.Extensions;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace FunctionalTests.Facts.MiddlewarePatterns
{
    public class ConvertibleMiddleware
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void ConvertibleMiddlewareTest(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy<ConvertibleMiddleWarePatternStartup>(hostType);
                Assert.Equal("SUCCESS", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
            }
        }
    }

    public class ConvertibleMiddleWarePatternStartup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.Use(new Func<AppFunc, AppFunc>(OwinDefaultMiddleWare));
            builder.Use(typeof(Alpha));
            builder.Use(typeof(Beta), "etc");
            builder.Use(new Func<AppFunc, AppFunc>(DefaultApplication));

            builder.AddSignatureConversion(Conversion1);
            builder.AddSignatureConversion(Conversion2);
        }

        private static readonly Func<AppFunc, Foo> Conversion1 = owin => new FooThatCallsOwin(owin);
        private static readonly Func<Foo, AppFunc> Conversion2 = foo => new OwinThatCallsFoo(foo).Invoke;

        public AppFunc OwinDefaultMiddleWare(AppFunc next)
        {
            return env =>
            {
                env.Add("OwinDefaultMiddleWare", "OwinDefaultMiddleWare");
                return next(env);
            };
        }

        public AppFunc DefaultApplication(AppFunc next)
        {
            return env =>
            {
                if (env.Get<string>("OwinDefaultMiddleWare") != "OwinDefaultMiddleWare" || env.Get<string>("Alpha") != "Alpha" || env.Get<string>("Beta") != "Beta")
                {
                    throw new Exception("Test failed to find appropriate custom value added by middleware");
                }

                OwinResponse response = new OwinResponse(env);
                return response.WriteAsync("SUCCESS");
            };
        }
    }

    public class FooCallData
    {
        public IDictionary<string, object> Environment { get; set; }
    }

    public abstract class Foo
    {
        public abstract Task Call(FooCallData data);
    }

    public class Alpha : Foo
    {
        private readonly Foo _next;

        public Alpha(Foo next)
        {
            _next = next;
        }

        public override Task Call(FooCallData data)
        {
            data.Environment.Add("Alpha", "Alpha");
            return _next.Call(data);
        }
    }

    public class Beta : Foo
    {
        private readonly Foo _next;
        private readonly string _etc;

        public Beta(Foo next, string etc)
        {
            _next = next;
            _etc = etc;
        }

        public override Task Call(FooCallData data)
        {
            data.Environment.Add("Beta", "Beta");
            return _next.Call(data);
        }
    }

    internal class FooThatCallsOwin : Foo
    {
        private readonly AppFunc _next;

        public FooThatCallsOwin(AppFunc next)
        {
            _next = next;
        }

        public override Task Call(FooCallData data)
        {
            var env = data.Environment;
            return _next.Invoke(env);
        }
    }

    internal class OwinThatCallsFoo
    {
        private readonly Foo _next;

        public OwinThatCallsFoo(Foo next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var data = new FooCallData() { Environment = env };
            return _next.Call(data);
        }
    }
}