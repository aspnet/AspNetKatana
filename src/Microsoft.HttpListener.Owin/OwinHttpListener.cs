//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.HttpListener.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// This wraps HttpListener and exposes it as an OWIN compatible server.
    /// </summary>
    internal class OwinHttpListener : IDisposable
    {
        private const int DefaultMaxAccepts = 10;
        private const int DefaultMaxRequests = 500;

        private System.Net.HttpListener listener;
        private IList<string> basePaths;
        private TimeSpan maxRequestLifetime;
        private AppFunc appFunc;
        private DisconnectHandler disconnectHandler;
        private IDictionary<string, object> capabilities;
        
        private PumpLimits pumpLimits;
        private int currentOutstandingAccepts;
        private int currentOutstandingRequests;

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

            this.appFunc = appFunc;
            this.listener = new System.Net.HttpListener();

            this.basePaths = new List<string>();

            foreach (string url in urls)
            {
                this.listener.Prefixes.Add(url);

                // Assume http(s)://+:9090/BasePath, including the first path slash.  May be empty. Must not end with a slash.
                string basePath = url.Substring(url.IndexOf('/', url.IndexOf("//") + 2));
                if (basePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    basePath = basePath.Substring(0, basePath.Length - 1);
                }

                // TODO: Escaping normalization?
                basePaths.Add(basePath);
            }

            this.capabilities = capabilities;
            this.disconnectHandler = new DisconnectHandler(this.listener);
            this.maxRequestLifetime = TimeSpan.FromMilliseconds(Timeout.Infinite); // .NET 4.5  Timeout.InfiniteTimeSpan;

            this.SetPumpLimits(DefaultMaxAccepts, DefaultMaxRequests);
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
            get
            {
                return this.maxRequestLifetime;
            }

            set
            {
                if (value <= TimeSpan.Zero && value != TimeSpan.FromMilliseconds(Timeout.Infinite) /* .NET 4.5 Timeout.InfiniteTimeSpan */)
                {
                    throw new ArgumentOutOfRangeException("value", value, string.Empty);
                }

                this.maxRequestLifetime = value;
            }
        }

        /// <summary>
        /// These are merged as one object because they should be swapped out atomically.
        /// This controls how many requests the server attempts to process concurrently.
        /// </summary>
        internal void SetPumpLimits(int maxAccepts, int maxRequests)
        {
            pumpLimits = new PumpLimits(maxAccepts, maxRequests);

            // Kick the pump in case we went from zero to non-zero limits.
            this.StartNextRequestAsync();
        }

        /// <summary>
        /// Starts the listener and request processing threads.
        /// </summary>
        internal void Start()
        {
            if (!this.listener.IsListening)
            {
                this.listener.Start();
                this.disconnectHandler.Initialize();
            }

            this.StartNextRequestAsync();
        }

        // Returns null when the server shuts down.
        private void StartNextRequestAsync()
        {
            if (!this.listener.IsListening)
            {
                // Shut down.
                return;
            }

            PumpLimits limits = this.pumpLimits;
            if (this.currentOutstandingAccepts >= limits.MaxOutstandingAccepts
                || this.currentOutstandingRequests >= pumpLimits.MaxOutstandingRequests - this.currentOutstandingAccepts)
            {
                return;
            }

            Interlocked.Increment(ref this.currentOutstandingAccepts);

            try
            {
                this.listener.GetContextAsync()
                    .Then(context =>
                    {
                        Interlocked.Decrement(ref this.currentOutstandingAccepts);
                        Interlocked.Increment(ref this.currentOutstandingRequests);
                        this.InvokeRequestReceivedNotice();
                        this.StartNextRequestAsync();
                        this.StartProcessingRequest(context);
                    }).Catch(errorInfo =>
                    {
                        Interlocked.Decrement(ref this.currentOutstandingAccepts);
                        // TODO: Log
                        // Assume the HttpListener instance is closed.
                        return errorInfo.Handled();
                    });
            }
            catch (HttpListenerException /*ex*/)
            {
                Interlocked.Decrement(ref this.currentOutstandingAccepts);
                // TODO: Katana#5 - Make sure any other kind of exception crashes the process rather than getting swallowed by the Task infrastructure.

                // Disabled: HttpListener.IsListening is not updated until the end of HttpListener.Dispose().
                // Debug.Assert(!this.listener.IsListening, "Error other than shutdown: " + ex.ToString());
                return; // Shut down
            }
        }
        private void StartProcessingRequest(HttpListenerContext context)
        {
            RequestLifetimeMonitor lifetime = new RequestLifetimeMonitor(context, this.MaxRequestLifetime);

            try
            {
                if (context.Request.IsSecureConnection)
                {
                    // TODO: When is this ever async? Do we need to make this lazy so we don't slow down requests that don't care?
                    context.Request.GetClientCertificateAsync()
                        .Then(cert => this.ContinueProcessingRequest(lifetime, context, cert))
                        .Catch(errorInfo =>
                        {
                            this.EndRequest(lifetime, errorInfo.Exception);
                            return errorInfo.Handled();
                        });
                }
                else
                {
                    this.ContinueProcessingRequest(lifetime, context, null);
                }
            }
            catch (Exception ex)
            {
                // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle.  Otherwise crash the process.
                // Abort the request context with a closed connection.
                this.EndRequest(lifetime, ex);
            }
        }

        private void ContinueProcessingRequest(RequestLifetimeMonitor lifetime, HttpListenerContext context, X509Certificate2 clientCert)
        {
            try
            {
                // TODO: Check request.ClientCertificateError if clientCert is null?
                string basePath = GetBasePath(context.Request.Url);
                OwinHttpListenerRequest owinRequest = new OwinHttpListenerRequest(context.Request, basePath, clientCert);
                OwinHttpListenerResponse owinResponse = new OwinHttpListenerResponse(context, owinRequest.Environment, lifetime);
                IDictionary<string, object> env = owinRequest.Environment;
                env[Constants.CallCancelledKey] = lifetime.Token;
                this.PopulateServerKeys(env, context);
                
                Task appTask = this.appFunc(env)
                    .Then(() =>
                    {
                        owinResponse.Close();
                        this.EndRequest(lifetime, null);
                    })
                    .Catch(errorInfo =>
                    {
                        this.EndRequest(lifetime, errorInfo.Exception);
                        return errorInfo.Handled();
                    });

                if (!appTask.IsCompleted)
                {
                    // Expensive, do not register for this notification unless the application is async.
                    CancellationToken ct = this.disconnectHandler.GetDisconnectToken(context);
                    ct.Register(() => lifetime.End(new HttpListenerException(0 /* TODO: Lookup error code for client disconnect */)));
                }
            }
            catch (Exception ex)
            {
                this.EndRequest(lifetime, ex);
            }
        }

        private void EndRequest(RequestLifetimeMonitor lifetime, Exception ex)
        {
            // TODO: Log the exception, if any
            // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle.  Otherwise crash the process.
            // Abort the request context with a closed connection.
            Interlocked.Decrement(ref this.currentOutstandingAccepts);
            lifetime.End(ex);
            this.StartNextRequestAsync();
        }

        // When the server is listening on multiple urls, we need to decide which one is the correct base path for this request.
        // Use longest match.
        // TODO: Escaping normalization? 
        // TODO: Partial matches false positives (/b vs. /bob)?
        private string GetBasePath(Uri uri)
        {
            string bestMatch = string.Empty;
            foreach (string basePath in basePaths)
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
            env.Add(Constants.ServerCapabilitiesKey, capabilities);
            env.Add(typeof(HttpListenerContext).FullName, context);
            env.Add(typeof(System.Net.HttpListener).FullName, this.listener);
        }

        /// <summary>
        /// Stops the server from listening for new requests.  Active requests will continue to be processed.
        /// </summary>
        internal void Stop()
        {
            try
            {
                this.listener.Stop();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void InvokeRequestReceivedNotice()
        {
            Action testHook = this.RequestReceivedNotice;
            if (testHook != null)
            {
                testHook();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Shuts down the listener, cancels all pending requests, and the disposes of the listener.
        /// </summary>
        /// <param name="disposing">True if this is being called from user code, false for the finalizer thread.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.listener.IsListening)
                {
                    this.listener.Stop();
                }

                ((IDisposable)this.listener).Dispose();
            }
        }
    }
}
