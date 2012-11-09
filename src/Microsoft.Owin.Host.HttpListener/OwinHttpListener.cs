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
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// This wraps HttpListener and exposes it as an OWIN compatible server.
    /// </summary>
    internal class OwinHttpListener : IDisposable
    {
        private const int DefaultMaxAccepts = 10;
        private const int DefaultMaxRequests = 500;

        private readonly System.Net.HttpListener _listener;
        private readonly IList<string> _basePaths;
        private readonly AppFunc _appFunc;
        private readonly DisconnectHandler _disconnectHandler;
        private readonly IDictionary<string, object> _capabilities;

        private TimeSpan _maxRequestLifetime;
        private PumpLimits _pumpLimits;
        private int _currentOutstandingAccepts;
        private int _currentOutstandingRequests;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListener"/> class.
        /// Creates a new server instance that will listen on the given addresses.  The server is not started here.
        /// </summary>
        internal OwinHttpListener(AppFunc appFunc, IEnumerable<string> urls, IDictionary<string, object> capabilities)
        {
            if (appFunc == null)
            {
                throw new ArgumentNullException("appFunc");
            }

            _appFunc = appFunc;
            _listener = new System.Net.HttpListener();

            _basePaths = new List<string>();

            foreach (var url in urls)
            {
                _listener.Prefixes.Add(url);

                // Assume http(s)://+:9090/BasePath, including the first path slash.  May be empty. Must not end with a slash.
                string basePath = url.Substring(url.IndexOf('/', url.IndexOf("//") + 2));
                if (basePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    basePath = basePath.Substring(0, basePath.Length - 1);
                }

                // TODO: Escaping normalization?
                _basePaths.Add(basePath);
            }

            _capabilities = capabilities;
            _disconnectHandler = new DisconnectHandler(_listener);
            _maxRequestLifetime = TimeSpan.FromMilliseconds(Timeout.Infinite); // .NET 4.5  Timeout.InfiniteTimeSpan;

            SetPumpLimits(DefaultMaxAccepts, DefaultMaxRequests);
        }

        /// <summary>
        /// Gets or sets a test hook that fires each time a request is received 
        /// </summary>
        internal Action RequestReceivedNotice { get; set; }

        /// <summary>
        /// Gets or sets how long a request may be outstanding.  The default is infinite.
        /// </summary>
        internal TimeSpan MaxRequestLifetime
        {
            get { return _maxRequestLifetime; }

            set
            {
                if (value <= TimeSpan.Zero && value != TimeSpan.FromMilliseconds(Timeout.Infinite) /* .NET 4.5 Timeout.InfiniteTimeSpan */)
                {
                    throw new ArgumentOutOfRangeException("value", value, string.Empty);
                }

                _maxRequestLifetime = value;
            }
        }

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

        private void StartProcessingRequest(HttpListenerContext context)
        {
            var lifetime = new RequestLifetimeMonitor(context, MaxRequestLifetime);

            try
            {
                string basePath = GetBasePath(context.Request.Url);
                var owinRequest = new OwinHttpListenerRequest(context.Request, basePath);
                var owinResponse = new OwinHttpListenerResponse(context, owinRequest.Environment, lifetime);
                IDictionary<string, object> env = owinRequest.Environment;
                env[Constants.CallCancelledKey] = lifetime.Token;
                PopulateServerKeys(env, context);

                CancellationToken ct = _disconnectHandler.GetDisconnectToken(context);
                ct.Register(() => lifetime.End(new HttpListenerException(Constants.ErrorConnectionNoLongerValid)));

                Task appTask = _appFunc(env)
                    .Then((Func<Task>)owinResponse.CompleteResponseAsync)
                    .Then(() =>
                    {
                        owinResponse.Close();
                        EndRequest(lifetime, null);
                    })
                    .Catch(errorInfo =>
                    {
                        EndRequest(lifetime, errorInfo.Exception);
                        return errorInfo.Handled();
                    });
            }
            catch (Exception ex)
            {
                EndRequest(lifetime, ex);
            }
        }

        private void EndRequest(RequestLifetimeMonitor lifetime, Exception ex)
        {
            // TODO: Log the exception, if any
            // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle.  Otherwise crash the process.
            // Abort the request context with a closed connection.
            Interlocked.Decrement(ref _currentOutstandingAccepts);
            lifetime.End(ex);
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

        private void PopulateServerKeys(IDictionary<string, object> env, HttpListenerContext context)
        {
            env.Add(Constants.VersionKey, Constants.OwinVersion);
            env.Add(Constants.ServerCapabilitiesKey, _capabilities);
            env.Add(typeof(HttpListenerContext).FullName, context);
            env.Add(typeof(System.Net.HttpListener).FullName, _listener);
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
