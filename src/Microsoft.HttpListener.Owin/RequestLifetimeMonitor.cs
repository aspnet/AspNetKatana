// <copyright file="RequestLifetimeMonitor.cs" company="Katana contributors">
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

        private readonly HttpListenerContext _context;
        private readonly CancellationTokenSource _cts;
        private readonly Timer _timeout;

        private int _requestState;

        internal RequestLifetimeMonitor(HttpListenerContext context, TimeSpan timeLimit)
        {
            _context = context;
            _cts = new CancellationTokenSource();
            // .NET 4.5: cts.CancelAfter(timeLimit);
            _timeout = new Timer(Cancel, this, timeLimit, TimeSpan.FromMilliseconds(Timeout.Infinite));
            _requestState = RequestInProgress;
        }

        internal CancellationToken Token
        {
            get { return _cts.Token; }
        }

        internal bool TryStartResponse()
        {
            return Interlocked.CompareExchange(ref _requestState, ResponseInProgress, RequestInProgress) == RequestInProgress;
        }

        internal bool TryFinishResponse()
        {
            return Interlocked.CompareExchange(ref _requestState, Completed, ResponseInProgress) == ResponseInProgress;
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
                _context.Response.Close();
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
                    _cts.Cancel();
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
            _timeout.Dispose();
            _cts.Dispose();
            int priorState = Interlocked.Exchange(ref _requestState, Completed);

            if (priorState == RequestInProgress)
            {
                // If the response has not started yet then we can send an error response before closing it.
                _context.Response.StatusCode = 500;
                _context.Response.ContentLength64 = 0;
                _context.Response.Headers.Clear();
                try
                {
                    _context.Response.Close();
                }
                catch (HttpListenerException)
                {
                }
            }
            else if (priorState == ResponseInProgress)
            {
                _context.Response.Abort();
            }
            else
            {
                Contract.Requires(priorState == Completed);

                // Clean up after exceptions in the shutdown process. No-op if Response.Close() succeeded.
                _context.Response.Abort();
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
