using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Owin;

namespace Katana.Server.HttpListenerWrapper
{
    public class OwinHttpListener : IDisposable
    {
        private HttpListener listener;

        public HttpListener Listener
        {
            get { return listener; }
        }

        // Test hook that fires each time a request is received
        public Action RequestReceivedNotice { get; set; }

        public OwinHttpListener()
            : this(new HttpListener()) 
        {
        }

        public OwinHttpListener(HttpListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }

            this.listener = listener;
        }

        public void StartProcessingRequests(AppDelegate appDelegate)
        {
            StartProcessingRequests(appDelegate, 10); // TODO: Smart default
        }

        // TODO: Do we have to provide both AppDelegate and AppTaskDelegate support?
        public void StartProcessingRequests(AppDelegate appDelegate, int activeThreads)
        {
            if (appDelegate == null)
            {
                throw new ArgumentNullException("appDelegate");
            }
            if (activeThreads < 1)
            {
                throw new ArgumentOutOfRangeException("activeThreads", activeThreads, "At least one active thread is required.");
            }
            if (!listener.IsListening)
            {
                listener.Start();
            }
            for (int i = 0; i < activeThreads; i++)
            {
                AcceptRequestAsync(appDelegate);
            }
        }

        private async void AcceptRequestAsync(AppDelegate appDelegate)
        {
            HttpListenerContext context = null;
            while ((context = await GetNextRequestAsync()) != null)
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
                        }
                    );
                    await tcs.Task;
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, "User Request Exception: " + ex.ToString());
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                    // TODO: Write the error text as the response body for debugging?
                }
            }
        }

        public void StartProcessingRequests(AppTaskDelegate appDelegate)
        {
            StartProcessingRequests(appDelegate, 10); // TODO: Smart default
        }

        public void StartProcessingRequests(AppTaskDelegate appDelegate, int activeThreads)
        {
            if (appDelegate == null)
            {
                throw new ArgumentNullException("appDelegate");
            }
            if (activeThreads < 1)
            {
                throw new ArgumentOutOfRangeException("activeThreads", activeThreads, "At least one active thread is required.");
            }
            if (!listener.IsListening)
            {
                listener.Start();
            }
            for (int i = 0; i < activeThreads; i++)
            {
                AcceptRequestAsync(appDelegate);
            }
        }

        private async void AcceptRequestAsync(AppTaskDelegate appDelegate)
        {
            HttpListenerContext context = null;
            while ((context = await GetNextRequestAsync()) != null)
            {
                OwinHttpListenerRequest owinRequest = new OwinHttpListenerRequest(context.Request);
                Tuple<string /* status */, 
                    IDictionary<String, string[]> /* headers */,
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
                    // TODO: Write the error text as the response body for debugging?
                }

                if (owinResponse != null)
                {
                    await ProcessOwinResponseAsync(context, owinResponse.Item1, owinResponse.Item2, owinResponse.Item3);
                }
            }
        }

        // Returns null when complete
        private async Task<HttpListenerContext> GetNextRequestAsync()
        {
            if (!listener.IsListening)
            {
                return null;
            }
            try
            {
                HttpListenerContext context = await Listener.GetContextAsync();

                InvokeRequestReceivedNotice();

                return context;
            }
            catch (HttpListenerException ex)
            {
                Debug.Assert(!listener.IsListening, "Error other than shutdown: " + ex.ToString());
                return null; // Shut down
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

        // TODO: Why does the responseStatus have to be one combine field rather than a response code and a reason phrase?
        private async Task ProcessOwinResponseAsync(HttpListenerContext context, string responseStatus, 
            IDictionary<string, string[]> responseHeaders, BodyDelegate responseBodyDelegate)
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
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)listener).Dispose();
            }
        }
    }
}
