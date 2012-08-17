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
    using System.Threading.Tasks;

    internal class RequestLifetimeMonitor : IDisposable
    {
        private const int RequestInProgress = 1;
        private const int ResponseInProgress = 2;
        private const int Completed = 3;

        private HttpListenerContext context;
        private TaskCompletionSource<object> tcs;
        private int requestState;
        private Timer timeout;

        internal RequestLifetimeMonitor(HttpListenerContext context, TaskCompletionSource<object> tcs, TimeSpan timeLimit)
        {
            this.context = context;
            this.tcs = tcs;
            this.timeout = new Timer(Cancel, this, timeLimit, TimeSpan.FromMilliseconds(Timeout.Infinite));
            this.requestState = RequestInProgress;
        }

        internal Task Task
        {
            get
            {
                return this.tcs.Task;
            }
        }

        internal bool TryStartResponse()
        {
            return Interlocked.CompareExchange(ref this.requestState, ResponseInProgress, RequestInProgress) == RequestInProgress;
        }

        private static void Cancel(object state)
        {
            RequestLifetimeMonitor monitor = (RequestLifetimeMonitor)state;
            monitor.End(new TimeoutException());
        }

        internal void End(Exception ex)
        {
            // Debug.Assert(false, "Request exception: " + ex.ToString());
            if (!this.tcs.Task.IsCompleted)
            {
                if (ex != null)
                {
                    this.tcs.TrySetException(ex);
                }
                else
                {
                    this.tcs.TrySetResult(null);
                }

                this.End();
            }
        }

        // The request completed successfully.  Cancel the token anyways so any registered listeners can do cleanup.
        internal void CompleteResponse()
        {
            Interlocked.Exchange(ref this.requestState, Completed);
            this.End(null);
        }

        private void End()
        {
            this.timeout.Dispose();
            int priorState = Interlocked.Exchange(ref this.requestState, Completed);

            if (priorState == RequestInProgress)
            {
                // If the response has not started yet then we can send an error response before closing it.
                this.context.Response.StatusCode = 500;
                this.context.Response.ContentLength64 = 0;
                this.context.Response.Close();
            }
            else if (priorState == ResponseInProgress)
            {
                // If the response had already started then the best we can do is abort the context.
                this.context.Response.Abort();
            }
            else
            {
                Contract.Requires(priorState == Completed);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                End();
            }
        }
    }
}
