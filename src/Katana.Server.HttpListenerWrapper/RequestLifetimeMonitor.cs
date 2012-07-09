//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Threading;

    internal class RequestLifetimeMonitor
    {
        private const int RequestInProgress = 1;
        private const int ResponseInProgress = 2;
        private const int Completed = 3;

        private static readonly Action<object> CancelDelegate = Cancel;

        private HttpListenerContext context;
        private CancellationTokenSource cts;
        private int requestState;

        internal RequestLifetimeMonitor(HttpListenerContext context, CancellationTokenSource cts)
        {
            this.context = context;
            this.cts = cts;

            this.requestState = RequestInProgress;

            cts.Token.Register(Cancel, this);
        }

        internal bool TryStartResponse()
        {
            return Interlocked.CompareExchange(ref this.requestState, ResponseInProgress, RequestInProgress) == RequestInProgress;
        }

        internal void Cancel(Exception ex)
        {
            // Debug.Assert(false, "Request exception: " + ex.ToString());
            if (!this.cts.IsCancellationRequested)
            {
                try
                {
                    // Will invoke Cancel(this)
                    this.cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (AggregateException agex)
                {
                    Debug.Assert(false, "Cancel exception: " + agex.ToString());
                }
            }
        }

        // The request completed succesfully.  Cancel the token anyways so any registered listeners can do cleanup.
        internal void CompleteResponse()
        {
            Interlocked.Exchange(ref this.requestState, Completed);
            Cancel(null);
        }

        private static void Cancel(object state)
        {
            RequestLifetimeMonitor monitor = (RequestLifetimeMonitor)state;
            int priorState = Interlocked.Exchange(ref monitor.requestState, Completed);

            if (priorState == RequestInProgress)
            {
                // If the response has not started yet then we can send an error response before closing it.
                monitor.context.Response.StatusCode = 500;
                monitor.context.Response.ContentLength64 = 0;
                monitor.context.Response.Close();
            }
            else if (priorState == ResponseInProgress)
            {
                // If the response had already started then the best we can do is abort the context.
                monitor.context.Response.Abort();
            }
            else
            {
                Contract.Requires(priorState == Completed);
            }
        }
    }
}
