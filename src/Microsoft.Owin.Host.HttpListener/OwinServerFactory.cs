// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener
{
    using AddressList = IList<IDictionary<string, object>>;
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using CapabilitiesDictionary = IDictionary<string, object>;
    using LoggerFactoryFunc = Func<string, Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool>>;
    using LoggerFunc = Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool>;

    /// <summary>
    /// Implements the Katana setup pattern for the OwinHttpListener server.
    /// </summary>
    public static class OwinServerFactory
    {
        /// <summary>
        /// Advertise the capabilities of the server.
        /// </summary>
        /// <param name="properties"></param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by server later.")]
        public static void Initialize(IDictionary<string, object> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            properties[Constants.VersionKey] = Constants.OwinVersion;

            CapabilitiesDictionary capabilities =
                properties.Get<CapabilitiesDictionary>(Constants.ServerCapabilitiesKey)
                    ?? new Dictionary<string, object>();
            properties[Constants.ServerCapabilitiesKey] = capabilities;

            DetectWebSocketSupport(properties);

            // Let users set advanced configurations directly.
            var wrapper = new OwinHttpListener();
            properties[typeof(OwinHttpListener).FullName] = wrapper;
            properties[typeof(System.Net.HttpListener).FullName] = wrapper.Listener;
        }

        private static void DetectWebSocketSupport(IDictionary<string, object> properties)
        {
            // There is no explicit API to detect server side websockets, just check for v4.5 / Win8.
            // Per request we can provide actual verification.
            if (Environment.OSVersion.Version >= new Version(6, 2))
            {
                var capabilities = properties.Get<CapabilitiesDictionary>(Constants.ServerCapabilitiesKey);
                capabilities[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
            }
            else
            {
                var loggerFactory = properties.Get<LoggerFactoryFunc>(Constants.ServerLoggerFactoryKey);
                LoggerFunc logger = LogHelper.CreateLogger(loggerFactory, typeof(OwinServerFactory));
                LogHelper.LogInfo(logger, "No WebSocket support detected, Windows 8 or later is required.");
            }
        }

        /// <summary>
        /// Creates an OwinHttpListener and starts listening on the given URL.
        /// </summary>
        /// <param name="app">The application entry point.</param>
        /// <param name="properties">The addresses to listen on.</param>
        /// <returns>The OwinHttpListener.  Invoke Dispose to shut down.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        public static IDisposable Create(AppFunc app, IDictionary<string, object> properties)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            // Retrieve the instances created in Initialize
            OwinHttpListener wrapper = properties.Get<OwinHttpListener>(typeof(OwinHttpListener).FullName)
                ?? new OwinHttpListener();
            System.Net.HttpListener listener = properties.Get<System.Net.HttpListener>(typeof(System.Net.HttpListener).FullName)
                ?? new System.Net.HttpListener();

            AddressList addresses = properties.Get<AddressList>(Constants.HostAddressesKey)
                ?? new List<IDictionary<string, object>>();

            CapabilitiesDictionary capabilities =
                properties.Get<CapabilitiesDictionary>(Constants.ServerCapabilitiesKey)
                    ?? new Dictionary<string, object>();

            var loggerFactory = properties.Get<LoggerFactoryFunc>(Constants.ServerLoggerFactoryKey);

            wrapper.Start(listener, app, addresses, capabilities, loggerFactory);
            return wrapper;
        }
    }
}
