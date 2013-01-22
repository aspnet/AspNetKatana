// <copyright file="OwinHttpListenerResponse.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable RedundantUsingDirective
// ReSharper restore RedundantUsingDirective

namespace Microsoft.Owin.Host.HttpListener
{
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task /* Complete */>;

    /// <summary>
    /// This wraps an HttpListenerResponse, populates it with the given response fields, and relays 
    /// the response body to the underlying stream.
    /// </summary>
    internal class OwinHttpListenerResponse
    {
        private const int RequestInProgress = 1;
        private const int ResponseInProgress = 2;
        private const int Completed = 3;

        private readonly CallEnvironment _environment;
        private readonly HttpListenerResponse _response;

        private readonly HttpListenerContext _context;
        private bool _responsePrepared;
        private IList<Tuple<Action<object>, object>> _onSendingHeadersActions;
        private int _requestState;
#if !NET40
        private IDictionary<string, object> _acceptOptions;
        private WebSocketFunc _webSocketFunc;
        private Task _webSocketAction;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerResponse"/> class.
        /// Sets up the Environment with the necessary request state items.
        /// </summary>
        public OwinHttpListenerResponse(HttpListenerContext context, CallEnvironment environment)
        {
            Contract.Requires(context != null);
            Contract.Requires(environment != null);
            _context = context;
            _response = _context.Response;
            _environment = environment;

            _requestState = RequestInProgress;

            var outputStream = new HttpListenerStreamWrapper(_response.OutputStream);
            outputStream.OnFirstWrite = ResponseBodyStarted;
            _environment.ResponseBody = outputStream;

            _environment.ResponseHeaders = new ResponseHeadersDictionary(_response);

            _onSendingHeadersActions = new List<Tuple<Action<object>, object>>();
            _environment.OnSendingHeaders = RegisterForOnSendingHeaders;
        }

#if !NET40
        private void DoWebSocketUpgrade(IDictionary<string, object> acceptOptions, WebSocketFunc callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            _environment[Constants.ResponseStatusCodeKey] = 101;
            _acceptOptions = acceptOptions;
            _webSocketFunc = callback;

            string subProtocol = GetWebSocketSubProtocol();

            PrepareResponse();

            // TODO: Other parameters?
            _webSocketAction = _context.AcceptWebSocketAsync(subProtocol)
                .Then(webSocketContext =>
                {
                    var wrapper = new WebSockets.OwinWebSocketWrapper(webSocketContext,
                        _environment.Get<CancellationToken>(Constants.CallCancelledKey));
                    return _webSocketFunc(wrapper.Environment)
                        .Then(() => wrapper.CleanupAsync());
                });
        }
#endif

        internal bool TryStartResponse()
        {
            return Interlocked.CompareExchange(ref _requestState, ResponseInProgress, RequestInProgress) == RequestInProgress;
        }

        internal bool TryFinishResponse()
        {
            return Interlocked.CompareExchange(ref _requestState, Completed, ResponseInProgress) == ResponseInProgress;
        }

        private void ResponseBodyStarted()
        {
            PrepareResponse();

            if (!TryStartResponse())
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        internal Task CompleteResponseAsync()
        {
            PrepareResponse();
#if NET40
            return TaskHelpers.Completed();
#else
            // Wait for the websocket callback to complete, if any
            return _webSocketAction ?? TaskHelpers.Completed();
#endif
        }

        // The request completed successfully.
        public void Close()
        {
            TryStartResponse();

            if (TryFinishResponse())
            {
                _context.Response.Close();
            }
        }

        // Set the status code and reason phrase from the environment.
        private void PrepareResponse()
        {
            if (_responsePrepared)
            {
                return;
            }

            _responsePrepared = true;

            NotifyOnSendingHeaders();

            SetStatusCode();

            SetReasonPhrase();

            // response.ProtocolVersion is ignored by Http.Sys.  It always sends 1.1
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "By design")]
        private void SetStatusCode()
        {
            object temp;
            if (_environment.TryGetValue(Constants.ResponseStatusCodeKey, out temp))
            {
                var statusCode = (int)temp;
                if (statusCode == 100 || statusCode < 100 || statusCode >= 1000)
                {
                    throw new ArgumentOutOfRangeException(Constants.ResponseStatusCodeKey, statusCode, string.Empty);
                }

                // Status
                _response.StatusCode = statusCode;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "By design")]
        private void SetReasonPhrase()
        {
            object reasonPhrase;
            if (_environment.TryGetValue(Constants.ResponseReasonPhraseKey, out reasonPhrase)
                && !string.IsNullOrWhiteSpace(reasonPhrase as String))
            {
                _response.StatusDescription = (string)reasonPhrase;
            }
        }

        private void RegisterForOnSendingHeaders(Action<object> callback, object state)
        {
            IList<Tuple<Action<object>, object>> actions = _onSendingHeadersActions;
            if (actions == null)
            {
                throw new InvalidOperationException("Headers already sent");
            }

            actions.Add(new Tuple<Action<object>, object>(callback, state));
        }

        private void NotifyOnSendingHeaders()
        {
            IList<Tuple<Action<object>, object>> actions = Interlocked.Exchange(ref _onSendingHeadersActions, null);
            Contract.Assert(actions != null);

            // Execute last to first. This mimics a stack unwind.
            for (int i = actions.Count - 1; i >= 0; i--)
            {
                Tuple<Action<object>, object> actionPair = actions[i];
                actionPair.Item1(actionPair.Item2);
            }
        }

#if !NET40
        private string GetWebSocketSubProtocol()
        {
            var reponseHeaders = _environment.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);

            // Remove the subprotocol header, Accept will re-add it.
            string subProtocol = null;
            string[] subProtocols;
            if (reponseHeaders.TryGetValue(Constants.SecWebSocketProtocol, out subProtocols) && subProtocols.Length > 0)
            {
                subProtocol = subProtocols[0];
                reponseHeaders.Remove(Constants.SecWebSocketProtocol);
            }

            if (_acceptOptions != null && _acceptOptions.ContainsKey(Constants.WebSocketSubProtocolKey))
            {
                subProtocol = _acceptOptions.Get<string>(Constants.WebSocketSubProtocolKey);
            }

            return subProtocol;
        }
#endif

        internal void End()
        {
            int priorState = Interlocked.Exchange(ref _requestState, Completed);

            if (priorState == RequestInProgress)
            {
                // Premature ending, must be an error.
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

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Only implemented for 4.5+")]
        internal bool TryGetWebSocketAccept(ref WebSocketAccept websocketAccept)
        {
#if !NET40
            if (_context.Request.IsWebSocketRequest)
            {
                websocketAccept = new WebSocketAccept(DoWebSocketUpgrade);
                return true;
            }
#endif
            websocketAccept = null;
            return false;
        }
    }
}
