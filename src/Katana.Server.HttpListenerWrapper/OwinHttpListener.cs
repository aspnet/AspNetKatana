namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Owin;

    /// <summary>
    /// This wraps HttpListener and exposes it as an OWIN compatible server.
    /// </summary>
    public class OwinHttpListener : IDisposable
    {
        private HttpListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListener"/> class.
        /// Creates a new server instance that will listen on the given url.  The server is not started here.
        /// </summary>
        /// <param name="url">The scheme, host, port, and path on which to listen for requests.</param>
        public OwinHttpListener(string url)
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(url);
        }

        // Test hook that fires each time a request is received
        public Action RequestReceivedNotice { get; set; }

        public void StartProcessingRequests(AppDelegate appDelegate)
        {
            this.StartProcessingRequests(appDelegate, 10); // TODO: Katana#5 - Smart defaults, smarter message pump.
        }

        // TODO: Katana#2 - We only need to implement either AppDelegate or AppTaskDelegate, and then Katana will shim for us.
        // However, at the moment Katana only supports AppDelegate servers.  Note we may still want both implementations if
        // we can do something more efficient than the shim.
        public void StartProcessingRequests(AppDelegate appDelegate, int activeThreads)
        {
            if (appDelegate == null)
            {
                throw new ArgumentNullException("appDelegate");
            }

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
                this.AcceptRequestAsync(appDelegate);
            }
        }

        private async void AcceptRequestAsync(AppDelegate appDelegate)
        {
            HttpListenerContext context = null;
            while ((context = await this.GetNextRequestAsync()) != null)
            {
                OwinHttpListenerRequest owinRequest = new OwinHttpListenerRequest(context.Request);

                try
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                    appDelegate(
                        owinRequest.Environment,
                        async (status, headers, body) => 
                        {
                            await ProcessOwinResponseAsync(context, status, headers, body);
                            tcs.TrySetResult(null);
                        }, 
                        (ex) => 
                        {
                            if (ex != null)
                            {
                                tcs.TrySetException(ex);
                            }
                            else
                            {
                                tcs.TrySetResult(null);
                            }
                        });
                    await tcs.Task;
                }
                catch (Exception ex)
                {
                    // TODO: Katana#5 - Don't catch everything, only catch what we think we can handle.  Otherwise crash the process.
                    Debug.Assert(false, "User Request Exception: " + ex.ToString());
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
        }

        public void StartProcessingRequests(AppTaskDelegate appDelegate)
        {
            this.StartProcessingRequests(appDelegate, 10); // TODO: Katana#5 - Smart defaults, smarter message pump.
        }

        public void StartProcessingRequests(AppTaskDelegate appDelegate, int activeThreads)
        {
            if (appDelegate == null)
            {
                throw new ArgumentNullException("appDelegate");
            }

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
                this.AcceptRequestAsync(appDelegate);
            }
        }

        private async void AcceptRequestAsync(AppTaskDelegate appDelegate)
        {
            HttpListenerContext context = null;
            while ((context = await this.GetNextRequestAsync()) != null)
            {
                OwinHttpListenerRequest owinRequest = new OwinHttpListenerRequest(context.Request);
                Tuple<string /* status */, 
                    IDictionary<string, string[]> /* headers */,
                    BodyDelegate /* bBodyDelegate */> owinResponse = null;

                try
                {
                    owinResponse = await appDelegate(owinRequest.Environment);
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, "User Request Exception: " + ex.ToString());
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }

                if (owinResponse != null)
                {
                    await this.ProcessOwinResponseAsync(context, owinResponse.Item1, owinResponse.Item2, owinResponse.Item3);
                }
            }
        }

        // Returns null when complete
        private async Task<HttpListenerContext> GetNextRequestAsync()
        {
            if (!this.listener.IsListening)
            {
                return null;
            }

            try
            {
                HttpListenerContext context = await this.listener.GetContextAsync();

                this.InvokeRequestReceivedNotice();

                return context;
            }
            catch (HttpListenerException ex)
            {
                // TODO: Katana#5 - Make sure any other kind of exception crashes the process rather than getting swallowed by the Task infrastructure.
                Debug.Assert(!this.listener.IsListening, "Error other than shutdown: " + ex.ToString());
                return null; // Shut down
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

        // TODO: Why does the responseStatus have to be one combine field rather than a response code and a reason phrase?
        private async Task ProcessOwinResponseAsync(
            HttpListenerContext context, 
            string responseStatus, 
            IDictionary<string, string[]> responseHeaders, 
            BodyDelegate responseBodyDelegate)
        {
            OwinHttpListenerResponse owinResponse = new OwinHttpListenerResponse(context.Response, responseStatus, responseHeaders);

            // Body
            if (responseBodyDelegate == null)
            {
                context.Response.Close();
            }
            else
            {
                try
                {
                    responseBodyDelegate(owinResponse.Write, owinResponse.End, CancellationToken.None);
                    await owinResponse.Completion;
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, "User Response Exception: " + ex.ToString());
                    context.Response.Close();
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)this.listener).Dispose();
            }
        }
    }
}
