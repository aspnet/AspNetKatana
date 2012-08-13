//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Owin;

    /// <summary>
    /// This wraps HttpListener and exposes it as an OWIN compatible server.
    /// </summary>
    public class OwinHttpListener : IDisposable
    {
        private HttpListener listener;
        private string basePath;
        private TimeSpan maxRequestLifetime;
        private TaskCompletionSource<object> allRequestCancellation;
        private AppDelegate appDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListener"/> class.
        /// Creates a new server instance that will listen on the given url.  The server is not started here.
        /// </summary>
        /// <param name="appDelegate">The application entry point.</param>
        /// <param name="url">The scheme, host, port, and path on which to listen for requests.</param>
        public OwinHttpListener(AppDelegate appDelegate, string url)
        {
            if (appDelegate == null)
            {
                throw new ArgumentNullException("appDelegate");
            }

            this.appDelegate = appDelegate;
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(url);

            // Assume http(s)://+:9090/BasePath, including the first path slash.  May be empty. Must not end with a slash.
            this.basePath = url.Substring(url.IndexOf('/', url.IndexOf("//") + 2));
            if (this.basePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                this.basePath = this.basePath.Substring(0, this.basePath.Length - 1);
            }

            this.maxRequestLifetime = Timeout.InfiniteTimeSpan;
            this.allRequestCancellation = new TaskCompletionSource<object>();
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
                if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
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
                this.AcceptRequestsAsync();
            }
        }

        private async void AcceptRequestsAsync()
        {
            HttpListenerContext context = null;
            while ((context = await this.GetNextRequestAsync()) != null)
            {
                TaskCompletionSource<object> tcs = this.GetRequestLifetimeToken();

                using (RequestLifetimeMonitor lifetime = new RequestLifetimeMonitor(context, tcs, this.MaxRequestLifetime))
                {
                    ResultParameters result = default(ResultParameters);
                    try
                    {
                        X509Certificate2 clientCert = null;
                        if (context.Request.IsSecureConnection)
                        {
                            clientCert = await context.Request.GetClientCertificateAsync();
                        }

                        OwinHttpListenerRequest owinRequest = new OwinHttpListenerRequest(context.Request, this.basePath, clientCert);
                        CallParameters requestParameters = owinRequest.AppParameters;
                        requestParameters.Environment[Constants.CallCompletedKey] = tcs.Task;
                        this.PopulateServerKeys(requestParameters, context);
                        result = await this.appDelegate(requestParameters);
                    }
                    catch (Exception ex)
                    {
                        // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle.  Otherwise crash the process.
                        // Abort the request context with a default error code (500).
                        lifetime.End(ex);
                    }

                    // Prepare and send the response now.  If there is a failure at this point we must reset the connection.
                    try
                    {
                        // Has the request failed or been canceled yet?
                        if (lifetime.TryStartResponse())
                        {
                            OwinHttpListenerResponse owinResponse = new OwinHttpListenerResponse(context, result);
                            await owinResponse.ProcessBodyAsync();
                            lifetime.CompleteResponse();
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle.  Otherwise crash the process.
                        // Abort the request context with a closed connection.
                        lifetime.End(ex);
                    }
                }
            }
        }

        private void PopulateServerKeys(CallParameters requestParameters, HttpListenerContext context)
        {
            requestParameters.Environment.Add("httplistener.Version", "HttpListener .NET 4.5, OWIN wrapper 1.0");
            requestParameters.Environment.Add(typeof(HttpListenerContext).Name, context);
            requestParameters.Environment.Add(typeof(HttpListener).Name, this.listener);
            if (context.Request.IsWebSocketRequest)
            {
                requestParameters.Environment.Add(Constants.WebSocketSupportKey, Constants.WebSocketSupport);
            }
        }

        // Returns null when the server shuts down.
        private async Task<HttpListenerContext> GetNextRequestAsync()
        {
            if (!this.listener.IsListening)
            {
                // Shut down.
                return null;
            }

            try
            {
                HttpListenerContext context = await this.listener.GetContextAsync();

                this.InvokeRequestReceivedNotice();

                return context;
            }
            catch (HttpListenerException /*ex*/)
            {
                // TODO: Katana#5 - Make sure any other kind of exception crashes the process rather than getting swallowed by the Task infrastructure.

                // Disabled: HttpListener.IsListening is not updated until the end of HttpListener.Dispose().
                // Debug.Assert(!this.listener.IsListening, "Error other than shutdown: " + ex.ToString());
                return null; // Shut down
            }
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

        private TaskCompletionSource<object> GetRequestLifetimeToken()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            this.allRequestCancellation.Task.ContinueWith(t => tcs.TrySetResult(null));
            return tcs;
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

                this.allRequestCancellation.TrySetException(new ObjectDisposedException(GetType().FullName));

                ((IDisposable)this.listener).Dispose();
            }
        }
    }
}
