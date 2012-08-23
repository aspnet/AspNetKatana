using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebApi.Owin;
using Owin;

namespace Microsoft.AspNet.WebApi.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class Conversions
    {
        static Conversions()
        {
            ToAppDelegate = app => new CallAppDelegate(app);
            ToMessageHandler = handler => new CallMessageHandler(handler).Invoke;
        }

        public static Func<AppFunc, HttpMessageHandler> ToAppDelegate { get; set; }

        public static Func<HttpMessageHandler, AppFunc> ToMessageHandler { get; set; }
    }
}
