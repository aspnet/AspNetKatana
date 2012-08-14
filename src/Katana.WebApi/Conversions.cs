using System;
using System.Net.Http;
using Owin;

namespace Katana.WebApi
{
    public static class Conversions
    {
        static Conversions()
        {
            ToAppDelegate = app => new CallAppDelegate(app);
            ToMessageHandler = handler => new CallMessageHandler(handler).Send;
        }

        public static Func<AppDelegate, HttpMessageHandler> ToAppDelegate { get; set; }

        public static Func<HttpMessageHandler, AppDelegate> ToMessageHandler { get; set; }
    }
}
