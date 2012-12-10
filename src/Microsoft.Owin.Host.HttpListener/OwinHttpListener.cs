// <copyright file="OwinHttpListener.cs" company="Katana contributors">
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using HttpListener = System.Net.HttpListener;

    /// <summary>
    /// This wraps HttpListener and exposes it as an OWIN compatible server.
    /// </summary>
    internal class OwinHttpListener : IDisposable
    {
        private static readonly int DefaultMaxAccepts = 10 * Environment.ProcessorCount;
        private static readonly int DefaultMaxRequests = 100 * Environment.ProcessorCount;

        private readonly HttpListener _listener;
        private readonly IList<string> _basePaths;
        private readonly AppFunc _appFunc;
        private readonly DisconnectHandler _disconnectHandler;
        private readonly IDictionary<string, object> _capabilities;
        private readonly Action<int, int> _setPumpLimitsAction;

        private PumpLimits _pumpLimits;
        private int _currentOutstandingAccepts;
        private int _currentOutstandingRequests;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListener"/> class.
        /// Creates a new server instance that will listen on the given addresses.  The server is not started here.
        /// </summary>
        internal OwinHttpListener(HttpListener listener, AppFunc appFunc, IList<IDictionary<string, object>> addresses,
            IDictionary<string, object> capabilities)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            if (appFunc == null)
            {
                throw new ArgumentNullException("appFunc");
            }

            _listener = listener;
            _appFunc = appFunc;

            _basePaths = new List<string>();

            foreach (var address in addresses)
            {
                // build url from parts
                var scheme = address.Get<string>("scheme") ?? Uri.UriSchemeHttp;
                var host = address.Get<string>("host") ?? "+";
                var port = address.Get<string>("port") ?? "8080";
                var path = address.Get<string>("path") ?? string.Empty;

                // if port is present, add delimiter to value before concatenation
                if (!string.IsNullOrWhiteSpace(port))
                {
                    port = ":" + port;
                }

                // Assume http(s)://+:9090/BasePath, including the first path slash.  May be empty. Must not end with a slash.
                string basePath = path;
                if (basePath.EndsWith("/", StringComparison.Ordinal))
                {
                    basePath = basePath.Substring(0, basePath.Length - 1);
                }
                else
                {
                    // Http.Sys requires that the URL end in a slash
                    path += "/";
                }
                _basePaths.Add(basePath);

                // add a server for each url
                string url = scheme + "://" + host + port + path;
                _listener.Prefixes.Add(url);
            }

            _capabilities = capabilities;
            _disconnectHandler = new DisconnectHandler(_listener);

            _setPumpLimitsAction = new Action<int, int>(SetPumpLimits);

            SetPumpLimits(DefaultMaxAccepts, DefaultMaxRequests);
        }

        /// <summary>
        /// Gets or sets a test hook that fires each time a request is received 
        /// </summary>
        internal Action RequestReceivedNotice { get; set; }

        /// <summary>
        /// These are merged as one object because they should be swapped out atomically.
        /// This controls how many requests the server attempts to process concurrently.
        /// </summary>
        internal void SetPumpLimits(int maxAccepts, int maxRequests)
        {
            _pumpLimits = new PumpLimits(maxAccepts, maxRequests);

            // Kick the pump in case we went from zero to non-zero limits.
            OffloadStartNextRequest();
        }

        /// <summary>
        /// Starts the listener and request processing threads.
        /// </summary>
        internal void Start()
        {
            if (!_listener.IsListening)
            {
                _listener.Start();
                _disconnectHandler.Initialize();
            }

            OffloadStartNextRequest();
        }

        private void OffloadStartNextRequest()
        {
            Task.Factory.StartNew(StartNextRequestAsync);
        }

        // Returns null when the server shuts down.
        private void StartNextRequestAsync()
        {
            if (!_listener.IsListening)
            {
                // Shut down.
                return;
            }

            PumpLimits limits = _pumpLimits;
            if (_currentOutstandingAccepts >= limits.MaxOutstandingAccepts
                || _currentOutstandingRequests >= limits.MaxOutstandingRequests - _currentOutstandingAccepts)
            {
                return;
            }

            Interlocked.Increment(ref _currentOutstandingAccepts);

            try
            {
                _listener.GetContextAsync()
                    .Then(context =>
                    {
                        Interlocked.Decrement(ref _currentOutstandingAccepts);
                        Interlocked.Increment(ref _currentOutstandingRequests);
                        InvokeRequestReceivedNotice();
                        OffloadStartNextRequest();
                        StartProcessingRequest(context);
                    }).Catch(errorInfo =>
                    {
                        Interlocked.Decrement(ref _currentOutstandingAccepts);
                        // TODO: Log
                        // Assume the HttpListener instance is closed.
                        return errorInfo.Handled();
                    });
            }
            catch (HttpListenerException /*ex*/)
            {
                Interlocked.Decrement(ref _currentOutstandingAccepts);
                // TODO: Katana#5 - Make sure any other kind of exception crashes the process rather than getting swallowed by the Task infrastructure.

                // Disabled: HttpListener.IsListening is not updated until the end of HttpListener.Dispose().
                // Debug.Assert(!this.listener.IsListening, "Error other than shutdown: " + ex.ToString());
                return; // Shut down
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged")]
        private void StartProcessingRequest(HttpListenerContext context)
        {
            OwinHttpListenerContext owinContext = null;

            try
            {
                string basePath = GetBasePath(context.Request.Url);
                owinContext = new OwinHttpListenerContext(context, basePath, _disconnectHandler);
                PopulateServerKeys(owinContext.Environment);

                _appFunc(owinContext.Environment)
                    .Then((Func<Task>)owinContext.Response.CompleteResponseAsync)
                    .Then(() =>
                    {
                        owinContext.Response.Close();
                        EndRequest(owinContext, null);
                    })
                    .Catch(errorInfo =>
                    {
                        EndRequest(owinContext, errorInfo.Exception);
                        return errorInfo.Handled();
                    });
            }
            catch (Exception ex)
            {
                // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle?  Otherwise crash the process.
                EndRequest(owinContext, ex);
            }
        }

        private void EndRequest(OwinHttpListenerContext owinContext, Exception ex)
        {
            // TODO: Log the exception, if any
            Interlocked.Decrement(ref _currentOutstandingAccepts);
            if (owinContext != null)
            {
                owinContext.End(ex);
                owinContext.Dispose();
            }
            StartNextRequestAsync();
        }

        // When the server is listening on multiple urls, we need to decide which one is the correct base path for this request.
        // Use longest match.
        // TODO: Escaping normalization? 
        // TODO: Partial matches false positives (/b vs. /bob)?
        private string GetBasePath(Uri uri)
        {
            string bestMatch = string.Empty;
            foreach (var basePath in _basePaths)
            {
                if (uri.AbsolutePath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)
                    && basePath.Length > bestMatch.Length)
                {
                    bestMatch = basePath;
                }
            }

            return bestMatch;
        }

        private void PopulateServerKeys(CallEnvironment env)
        {
            env.ServerCapabilities = _capabilities;
            env.Listener = _listener;
            env.SetPumpLimits = _setPumpLimitsAction;
        }

        /// <summary>
        /// Stops the server from listening for new requests.  Active requests will continue to be processed.
        /// </summary>
        internal void Stop()
        {
            try
            {
                _listener.Stop();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void InvokeRequestReceivedNotice()
        {
            Action testHook = RequestReceivedNotice;
            if (testHook != null)
            {
                testHook();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Shuts down the listener, cancels all pending requests, and the disposes of the listener.
        /// </summary>
        /// <param name="disposing">True if this is being called from user code, false for the finalizer thread.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_listener.IsListening)
                {
                    _listener.Stop();
                }

                ((IDisposable)_listener).Dispose();
            }
        }
    }
}
