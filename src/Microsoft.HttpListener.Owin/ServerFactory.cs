//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Microsoft.HttpListener.Owin.ServerFactory]

namespace Microsoft.HttpListener.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Reflection;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// Implements the Katana setup pattern for the OwinHttpListener server.
    /// </summary>
    public class ServerFactory : Attribute
    {
        private static bool IsWebSocketSupported = false;

        public static void Initialize(IDictionary<string, object> properties)
        {
            properties[Constants.VersionKey] = Constants.OwinVersion;

            // Check for WebSockets support.
            // Requires Win8 and the Katana.Server.DotNetWebSockets.dll.
            DetectWebSocketSupport(properties);
        }

        private static void DetectWebSocketSupport(IDictionary<string, object> properties)
        {
            // There is no explicit API to detect server side websockets, just check for v4.5 / Win8.
            // Per request we can provide actual verification.
            if (Environment.OSVersion.Version >= new Version(6, 2))
            {
                try
                {
                    Assembly webSocketMiddlewareAssembly = Assembly.Load("Microsoft.WebSockets.Owin");

                    IsWebSocketSupported = true;

                    properties[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;
                }
                catch (Exception)
                {
                    // TODO: Trace
                }
            }
            else
            {
                // TODO: Trace
            }
        }

        private static AppFunc AddWebSocketMiddleware(AppFunc app)
        {
            try
            {
                Assembly webSocketMiddlewareAssembly = Assembly.Load("Microsoft.WebSockets.Owin");

                return (AppFunc)webSocketMiddlewareAssembly.GetType("Owin.WebSocketWrapperExtensions")
                    .GetMethod("HttpListenerMiddleware")
                    .Invoke(null, new object[] { app });
            }
            catch (Exception)
            {
                // TODO: Trace
                throw;
            }
        }

        /// <summary>
        /// Creates an OwinHttpListener and starts listening on the given url.
        /// </summary>
        /// <param name="app">The application entry point.</param>
        /// <param name="properties">The addresses to listen on.</param>
        /// <returns>The OwinHttpListener.  Invoke Dispose to shut down.</returns>
        public static IDisposable Create(AppFunc app, IDictionary<string, object> properties)
        {
            if (IsWebSocketSupported)
            {
                app = AddWebSocketMiddleware(app);
            }

            var addresses = properties.Get<IList<IDictionary<string, object>>>("host.Addresses");
            IList<string> urls = new List<string>();
            foreach (var address in addresses)
            {
                // build url from parts
                var scheme = address.Get<string>("scheme");
                var host = address.Get<string>("host");
                var port = address.Get<string>("port");
                var path = address.Get<string>("path");

                // if port is present, add delimiter to value before concatenation
                if (!string.IsNullOrWhiteSpace(port))
                    port = ":" + port;

                // add a server for each url
                var url = scheme + "://" + host + port + path + "/";
                urls.Add(url);
            }

            OwinHttpListener server = new OwinHttpListener(app, urls);
            server.Start();
            return server;
        }
    }
}
