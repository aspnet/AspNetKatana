//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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
        private IDictionary<string, object> environment;
        private HttpListenerContext context;
        private HttpListenerResponse response;
        private RequestLifetimeMonitor lifetime;
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
            this.response = context.Response;
            this.environment = environment;
            this.lifetime = lifetime;

            HttpListenerStreamWrapper outputStream = new HttpListenerStreamWrapper(this.response.OutputStream);
            outputStream.OnFirstWrite = ResponseBodyStarted;
            this.environment.Add(Constants.ResponseBodyKey, outputStream);

            ResponseHeadersDictionary headers = new ResponseHeadersDictionary(this.response);
            this.environment.Add(Constants.ResponseHeadersKey, headers);

            this.onSendingHeadersActions = new List<Tuple<Action<object>, object>>();
            this.environment.Add(Constants.ServerOnSendingHeadersKey, new Action<Action<object>, object>(RegisterForOnSendingHeaders));
        }
        
        private void ResponseBodyStarted()
        {
            this.PrepareResponse();

            if (!lifetime.TryStartResponse())
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        public void Close()
        {
            this.PrepareResponse();
            
            this.lifetime.TryStartResponse();

            if (this.lifetime.TryFinishResponse())
            {
                this.lifetime.CompleteResponse();
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
            if (this.environment.TryGetValue(Constants.ResponseStatusCodeKey, out temp))
            {
                int statusCode = (int)temp;
                if (statusCode == 100 || statusCode < 100 || statusCode >= 1000)
                {
                    throw new ArgumentOutOfRangeException(Constants.ResponseStatusCodeKey, statusCode, string.Empty);
                }

                // Status
                this.response.StatusCode = statusCode;
            }
        }

        private void SetReasonPhrase()
        {
            object reasonPhrase;
            if (this.environment.TryGetValue(Constants.ResponseReasonPhraseKey, out reasonPhrase)
                && !string.IsNullOrWhiteSpace((string)reasonPhrase))
            {
                this.response.StatusDescription = (string)reasonPhrase;
            }
        }

        private void RegisterForOnSendingHeaders(Action<object> callback, object state)
        {
            IList<Tuple<Action<object>, object>> actions = this.onSendingHeadersActions;
            if (actions == null)
            {
                throw new InvalidOperationException("Headers already sent");
            }

            actions.Add(new Tuple<Action<object>, object>(callback, state));
        }

        private void NotifyOnSendingHeaders()
        {
            var actions = Interlocked.Exchange(ref this.onSendingHeadersActions, null);
            Contract.Assert(actions != null);

            // Execute last to first. This mimics a stack unwind.
            foreach (var actionPair in actions.Reverse())
            {
                actionPair.Item1(actionPair.Item2);
            }
        }
    }
}
