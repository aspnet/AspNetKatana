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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading;

namespace Microsoft.HttpListener.Owin
{
    /// <summary>
    /// This wraps an HttpListenerResponse, populates it with the given response fields, and relays 
    /// the response body to the underlying stream.
    /// </summary>
    internal class OwinHttpListenerResponse
    {
        private readonly IDictionary<string, object> environment;
        private HttpListenerContext context;
        private readonly HttpListenerResponse response;
        private readonly RequestLifetimeMonitor lifetime;
        private bool responsePrepared;
        private IList<Tuple<Action<object>, object>> onSendingHeadersActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerResponse"/> class.
        /// Sets up the Environment with the necessary request state items.
        /// </summary>
        public OwinHttpListenerResponse(HttpListenerContext context, IDictionary<string, object> environment, RequestLifetimeMonitor lifetime)
        {
            Contract.Requires(context != null);
            Contract.Requires(environment != null);
            this.context = context;
            response = context.Response;
            this.environment = environment;
            this.lifetime = lifetime;

            HttpListenerStreamWrapper outputStream = new HttpListenerStreamWrapper(response.OutputStream);
            outputStream.OnFirstWrite = ResponseBodyStarted;
            this.environment.Add(Constants.ResponseBodyKey, outputStream);

            ResponseHeadersDictionary headers = new ResponseHeadersDictionary(response);
            this.environment.Add(Constants.ResponseHeadersKey, headers);

            onSendingHeadersActions = new List<Tuple<Action<object>, object>>();
            this.environment.Add(Constants.ServerOnSendingHeadersKey, new Action<Action<object>, object>(RegisterForOnSendingHeaders));
        }
        
        private void ResponseBodyStarted()
        {
            PrepareResponse();

            if (!lifetime.TryStartResponse())
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Close()
        {
            PrepareResponse();
            
            lifetime.TryStartResponse();

            if (lifetime.TryFinishResponse())
            {
                lifetime.CompleteResponse();
            }
        }

        // Set the status code and reason phrase from the environment.
        private void PrepareResponse()
        {
            if (responsePrepared)
            {
                return;
            }

            responsePrepared = true;

            NotifyOnSendingHeaders();

            SetStatusCode();

            SetReasonPhrase();

            // response.ProtocolVersion is ignored by Http.Sys.  It always sends 1.1
        }

        private void SetStatusCode()
        {
            object temp;
            if (environment.TryGetValue(Constants.ResponseStatusCodeKey, out temp))
            {
                int statusCode = (int)temp;
                if (statusCode == 100 || statusCode < 100 || statusCode >= 1000)
                {
                    throw new ArgumentOutOfRangeException(Constants.ResponseStatusCodeKey, statusCode, string.Empty);
                }

                // Status
                response.StatusCode = statusCode;
            }
        }

        private void SetReasonPhrase()
        {
            object reasonPhrase;
            if (environment.TryGetValue(Constants.ResponseReasonPhraseKey, out reasonPhrase)
                && !string.IsNullOrWhiteSpace((string)reasonPhrase))
            {
                response.StatusDescription = (string)reasonPhrase;
            }
        }

        private void RegisterForOnSendingHeaders(Action<object> callback, object state)
        {
            IList<Tuple<Action<object>, object>> actions = onSendingHeadersActions;
            if (actions == null)
            {
                throw new InvalidOperationException("Headers already sent");
            }

            actions.Add(new Tuple<Action<object>, object>(callback, state));
        }

        private void NotifyOnSendingHeaders()
        {
            var actions = Interlocked.Exchange(ref onSendingHeadersActions, null);
            Contract.Assert(actions != null);

            // Execute last to first. This mimics a stack unwind.
            foreach (var actionPair in actions.Reverse())
            {
                actionPair.Item1(actionPair.Item2);
            }
        }
    }
}
