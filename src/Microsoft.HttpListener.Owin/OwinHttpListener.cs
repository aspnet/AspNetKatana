//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.HttpListener.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// This wraps HttpListener and exposes it as an OWIN compatible server.
    /// </summary>
    public class OwinHttpListener : IDisposable
    {
        private HttpListener listener;
        private IList<string> basePaths;
        private TimeSpan maxRequestLifetime;
        private AppFunc appFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListener"/> class.
        /// Creates a new server instance that will listen on the given urls.  The server is not started here.
        /// </summary>
        /// <param name="appDelegate">The application entry point.</param>
        /// <param name="urls">The scheme, host, port, and path on which to listen for requests.</param>
        public OwinHttpListener(AppFunc appFunc, IEnumerable<string> urls)
        {
            if (appFunc == null)
            {
                throw new ArgumentNullException("appFunc");
            }

            this.appFunc = appFunc;
            this.listener = new HttpListener();

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

            this.maxRequestLifetime = TimeSpan.FromMilliseconds(Timeout.Infinite); // .NET 4.5  Timeout.InfiniteTimeSpan;
        }

        /// <summary>
        /// Gets or sets a test hook that fires each time a request is received 
        /// </summary>
        public Action RequestReceivedNotice { get; set; }

        /// <summary>
        /// Gets or sets how long a request may be outstanding.  The default is infinite.
        /// </summary>
        public TimeSpan MaxRequestLifetime
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
        /// Starts the listener and request processing threads.
        /// </summary>
        public void Start()
        {
            this.Start(10); // TODO: Katana#5 - Smart defaults, smarter message pump.
        }

        /// <summary>
        /// Starts the listener and request processing threads.
        /// </summary>
        /// <param name="activeThreads">The number of concurrent request processing threads to run.</param>
        public void Start(int activeThreads)
        {
            if (activeThreads < 1)
            {
                throw new ArgumentOutOfRangeException("activeThreads", activeThreads, string.Empty);
            }

            if (!this.listener.IsListening)
            {
                this.listener.Start();
            }

            for (int i = 0; i < activeThreads; i++)
            {
                this.GetNextRequestAsync();
            }
        }

        // Returns null when the server shuts down.
        private void GetNextRequestAsync()
        {
            if (!this.listener.IsListening)
            {
                // Shut down.
                return;
            }

            try
            {
                this.listener.GetContextAsync()
                    .Then(context =>
                    {
                        this.InvokeRequestReceivedNotice();
                        this.StartProcessingRequest(context);
                    }).Catch(errorInfo =>
                    {
                        // TODO: Log and assume the HttpListener instance is closed.
                        return errorInfo.Handled();
                    });
            }
            catch (HttpListenerException /*ex*/)
            {
                // TODO: Katana#5 - Make sure any other kind of exception crashes the process rather than getting swallowed by the Task infrastructure.

                // Disabled: HttpListener.IsListening is not updated until the end of HttpListener.Dispose().
                // Debug.Assert(!this.listener.IsListening, "Error other than shutdown: " + ex.ToString());
                return; // Shut down
            }
        }
        private void StartProcessingRequest(HttpListenerContext context)
        {
            if (context == null)
            {
                // Shut down
                return;
            }

            RequestLifetimeMonitor lifetime = new RequestLifetimeMonitor(context, this.MaxRequestLifetime);

            try
            {
                if (context.Request.IsSecureConnection)
                {
                    context.Request.GetClientCertificateAsync()
                        .Then(cert => this.ContinueProcessingRequest(lifetime, context, cert))
                        .Catch(errorInfo =>
                        {
                            // TODO: Log exception.
                            lifetime.End(errorInfo.Exception);
                            this.GetNextRequestAsync();
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
                lifetime.End(ex);
                this.GetNextRequestAsync();
            }
        }

        private void ContinueProcessingRequest(RequestLifetimeMonitor lifetime, HttpListenerContext context, X509Certificate2 clientCert)
        {
            try
            {
                string basePath = GetBasePath(context.Request.Url);
                OwinHttpListenerRequest owinRequest = new OwinHttpListenerRequest(context.Request, basePath, clientCert);
                OwinHttpListenerResponse owinResponse = new OwinHttpListenerResponse(context, owinRequest.Environment, lifetime);
                IDictionary<string, object> env = owinRequest.Environment;
                env[Constants.CallCancelledKey] = lifetime.Token;
                this.PopulateServerKeys(env, context);

                this.appFunc(env)
                    .Then(() =>
                    {
                        owinResponse.Close();
                        lifetime.CompleteResponse();
                        this.GetNextRequestAsync();
                    })
                    .Catch(errorInfo =>
                    {
                        // TODO: Log exception.
                        lifetime.End(errorInfo.Exception);
                        this.GetNextRequestAsync();
                        return errorInfo.Handled();
                    });
            }
            catch (Exception ex)
            {
                // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle.  Otherwise crash the process.
                // Abort the request context with a closed connection.
                lifetime.End(ex);
                this.GetNextRequestAsync();
            }
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
            env.Add(Constants.ServerNameKey, Constants.ServerName);
            env.Add(Constants.ServerVersionKey, Constants.ServerVersion);
            env.Add(typeof(HttpListenerContext).FullName, context);
            env.Add(typeof(HttpListener).FullName, this.listener);
        }

        /// <summary>
        /// Stops the server from listening for new requests.  Active requests will continue to be processed.
        /// </summary>
        public void Stop()
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

        /// <summary>
        /// See Dispose(bool)
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Shuts down the listener, cancels all pending requests, and the disposes of the listener.
        /// </summary>
        /// <param name="disposing">True if this is being called from user code, false for the finalizer thread.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
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
