// <copyright file="ServerFactory.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Owin;

[assembly: Microsoft.Owin.Host.HttpListener.ServerFactory]

namespace Microsoft.Owin.Host.HttpListener
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Implements the Katana setup pattern for the OwinHttpListener server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class ServerFactory : Attribute
    {
        /// <summary>
        /// Advertise the capabilities of the server.
        /// </summary>
        /// <param name="builder"></param>
        public static void Initialize(IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

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
                var capabilities = builder.Properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey);
                capabilities[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
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
                string url = scheme + "://" + host + port + path + "/";
                urls.Add(url);
            }

            IDictionary<string, object> capabilities =
                properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey)
                    ?? new Dictionary<string, object>();
            var server = new OwinHttpListener(app, urls, capabilities);
            server.Start();
            return server;
        }
    }
}
