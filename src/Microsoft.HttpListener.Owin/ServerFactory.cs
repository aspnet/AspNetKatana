//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Owin;

[assembly: Microsoft.HttpListener.Owin.ServerFactory]

namespace Microsoft.HttpListener.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Implements the Katana setup pattern for the OwinHttpListener server.
    /// </summary>
    public class ServerFactory : Attribute
    {
        public static void Initialize(IAppBuilder builder)
        {
            builder.Properties[Constants.VersionKey] = Constants.OwinVersion;

            IDictionary<string, object> capabilities = 
                builder.Properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey) 
                ?? new Dictionary<string, object>();
            builder.Properties[Constants.ServerCapabilitiesKey] = capabilities;

            capabilities[Constants.ServerNameKey] = Constants.ServerName;
            capabilities[Constants.ServerVersionKey] = Constants.ServerVersion;

            DetectWebSocketSupport(builder);
        }

        private static void DetectWebSocketSupport(IAppBuilder builder)
        {
            // There is no explicit API to detect server side websockets, just check for v4.5 / Win8.
            // Per request we can provide actual verification.
            if (Environment.OSVersion.Version >= new Version(6, 2))
            {
                try
                {
                    Assembly webSocketMiddlewareAssembly = Assembly.Load("Microsoft.WebSockets.Owin");

                    webSocketMiddlewareAssembly.GetType("Owin.WebSocketWrapperExtensions")
                        .GetMethod("UseWebSocketWrapper")
                        .Invoke(null, new object[] { builder });
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

        /// <summary>
        /// Creates an OwinHttpListener and starts listening on the given url.
        /// </summary>
        /// <param name="app">The application entry point.</param>
        /// <param name="properties">The addresses to listen on.</param>
        /// <returns>The OwinHttpListener.  Invoke Dispose to shut down.</returns>
        public static IDisposable Create(AppFunc app, IDictionary<string, object> properties)
        {
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
                {
                    port = ":" + port;
                }

                // add a server for each url
                var url = scheme + "://" + host + port + path + "/";
                urls.Add(url);
            }

            var capabilities =
                properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey)
                ?? new Dictionary<string, object>();
            OwinHttpListener server = new OwinHttpListener(app, urls, capabilities);
            server.Start();
            return server;
        }
    }
}
