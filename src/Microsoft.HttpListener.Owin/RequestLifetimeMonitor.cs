// Copyright 2011-2012 Katana contributors
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
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;

namespace Microsoft.HttpListener.Owin
{
    internal class RequestLifetimeMonitor : IDisposable
    {
        private const int RequestInProgress = 1;
        private const int ResponseInProgress = 2;
        private const int Completed = 3;

        private readonly HttpListenerContext context;
        private readonly CancellationTokenSource cts;
        private int requestState;
        private readonly Timer timeout;

        internal RequestLifetimeMonitor(HttpListenerContext context, TimeSpan timeLimit)
        {
            this.context = context;
            cts = new CancellationTokenSource();
            // .NET 4.5: cts.CancelAfter(timeLimit);
            timeout = new Timer(Cancel, this, timeLimit, TimeSpan.FromMilliseconds(Timeout.Infinite));
            requestState = RequestInProgress;
        }

        internal CancellationToken Token
        {
            get
            {
                return cts.Token;
            }
        }

        internal bool TryStartResponse()
        {
            return Interlocked.CompareExchange(ref requestState, ResponseInProgress, RequestInProgress) == RequestInProgress;
        }

        internal bool TryFinishResponse()
        {
            return Interlocked.CompareExchange(ref requestState, Completed, ResponseInProgress) == ResponseInProgress;
        }

        private static void Cancel(object state)
        {
            RequestLifetimeMonitor monitor = (RequestLifetimeMonitor)state;
            monitor.End(new TimeoutException());
        }

        // The request completed successfully.  Cancel the token anyways so any registered listeners can do cleanup.
        internal void CompleteResponse()
        {
            try
            {
                context.Response.Close();
                End(null);
            }
            catch (InvalidOperationException ioe)
            {
                // Content-Length, not enough bytes written
                End(ioe);
            }
            catch (HttpListenerException ex)
            {
                // TODO: Log
                End(ex);
            }
        }

        internal void End(Exception ex)
        {
            // Debug.Assert(false, "Request exception: " + ex.ToString());
            
            if (ex != null)
            {
                // TODO: LOG

                try
                {
                    cts.Cancel();
                }
                catch (ObjectDisposedException)
                { 
                }
                catch (AggregateException)
                {
                    // TODO: LOG
                }
            }

            End();
        }

        private void End()
        {
            timeout.Dispose();
            cts.Dispose();
            int priorState = Interlocked.Exchange(ref requestState, Completed);

            if (priorState == RequestInProgress)
            {
                // If the response has not started yet then we can send an error response before closing it.
                context.Response.StatusCode = 500;
                context.Response.ContentLength64 = 0;
                context.Response.Headers.Clear();
                try
                {
                    context.Response.Close();
                }
                catch (HttpListenerException)
                {
                }
            }
            else if (priorState == ResponseInProgress)
            {
                context.Response.Abort();
            }
            else
            {
                Contract.Requires(priorState == Completed);

                // Clean up after exceptions in the shutdown process. No-op if Response.Close() succeeded.
                context.Response.Abort();
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
