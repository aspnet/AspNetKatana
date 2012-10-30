// <copyright file="OwinHttpListenerResponse.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
#if NET45
using System.Net.WebSockets;
#endif
using System.Threading;
using System.Threading.Tasks;
#if NET45
using Microsoft.Owin.Host.HttpListener.WebSockets;
#endif

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
        private readonly IDictionary<string, object> _environment;
        private readonly HttpListenerResponse _response;
        private readonly RequestLifetimeMonitor _lifetime;

        private HttpListenerContext _context;
        private bool _responsePrepared;
        private IList<Tuple<Action<object>, object>> _onSendingHeadersActions;
#if NET45
        private IDictionary<string, object> _acceptOptions;
        private WebSocketFunc _webSocketFunc;
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerResponse"/> class.
        /// Sets up the Environment with the necessary request state items.
        /// </summary>
        public OwinHttpListenerResponse(HttpListenerContext context, IDictionary<string, object> environment, RequestLifetimeMonitor lifetime)
        {
            Contract.Requires(context != null);
            Contract.Requires(environment != null);
            _context = context;
            _response = context.Response;
            _environment = environment;
            _lifetime = lifetime;

            HttpListenerStreamWrapper outputStream = new HttpListenerStreamWrapper(_response.OutputStream);
            outputStream.OnFirstWrite = ResponseBodyStarted;
            _environment.Add(Constants.ResponseBodyKey, outputStream);

            ResponseHeadersDictionary headers = new ResponseHeadersDictionary(_response);
            _environment.Add(Constants.ResponseHeadersKey, headers);

            _onSendingHeadersActions = new List<Tuple<Action<object>, object>>();
            _environment.Add(Constants.ServerOnSendingHeadersKey, new Action<Action<object>, object>(RegisterForOnSendingHeaders));

#if NET45
            if (context.Request.IsWebSocketRequest)
            {
                _environment[Constants.WebSocketAcceptKey] = new WebSocketAccept(
                    (options, callback) =>
                    {
                        _environment[Constants.ResponseStatusCodeKey] = 101;
                        _acceptOptions = options;
                        _webSocketFunc = callback;
                    });
            }
#endif
        }

        private void ResponseBodyStarted()
        {
            PrepareResponse();

            if (!_lifetime.TryStartResponse())
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        internal Task CompleteResponseAsync()
        {
            PrepareResponse();
#if NET45
            if (_lifetime.TryStartResponse() 
                && _response.StatusCode == 101
                && _webSocketFunc != null)
            {
                string subProtocol = GetWebSocketSubProtocol();

                // TODO: Other parameters?
                return _context.AcceptWebSocketAsync(subProtocol)
                    .Then(webSocketContext =>
                    {
                        OwinWebSocketWrapper wrapper = new OwinWebSocketWrapper(webSocketContext, 
                            _environment.Get<CancellationToken>(Constants.CallCancelledKey));
                        return _webSocketFunc(wrapper.Environment)
                            .Then(() => wrapper.CleanupAsync());
                    });
            }
#endif
            return TaskHelpers.Completed();
        }

        public void Close()
        {
            _lifetime.TryStartResponse();

            if (_lifetime.TryFinishResponse())
            {
                _lifetime.CompleteResponse();
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

        private void SetStatusCode()
        {
            object temp;
            if (_environment.TryGetValue(Constants.ResponseStatusCodeKey, out temp))
            {
                int statusCode = (int)temp;
                if (statusCode == 100 || statusCode < 100 || statusCode >= 1000)
                {
                    throw new ArgumentOutOfRangeException(Constants.ResponseStatusCodeKey, statusCode, string.Empty);
                }

                // Status
                _response.StatusCode = statusCode;
            }
        }

        private void SetReasonPhrase()
        {
            object reasonPhrase;
            if (_environment.TryGetValue(Constants.ResponseReasonPhraseKey, out reasonPhrase)
                && !string.IsNullOrWhiteSpace((string)reasonPhrase))
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
            var actions = Interlocked.Exchange(ref _onSendingHeadersActions, null);
            Contract.Assert(actions != null);

            // Execute last to first. This mimics a stack unwind.
            foreach (var actionPair in actions.Reverse())
            {
                actionPair.Item1(actionPair.Item2);
            }
        }
#if NET45
        private string GetWebSocketSubProtocol()
        {
            IDictionary<string, string[]> reponseHeaders = _environment.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);

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
    }
}
