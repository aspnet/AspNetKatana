// <copyright file="OwinWebSocketWrapper.cs" company="Katana contributors">
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

#if NET45

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.SystemWeb.WebSockets
{
    using WebSocketCloseAsync =
        Func<int /* closeStatus */,
            string /* closeDescription */,
            CancellationToken /* cancel */,
            Task>;
    using WebSocketReceiveAsync =
        Func<ArraySegment<byte> /* data */,
            CancellationToken /* cancel */,
            Task<Tuple<int /* messageType */,
                bool /* endOfMessage */,
                int /* count */>>>;
    using WebSocketReceiveTuple =
        Tuple<int /* messageType */,
            bool /* endOfMessage */,
            int /* count */>;
    using WebSocketSendAsync =
        Func<ArraySegment<byte> /* data */,
            int /* messageType */,
            bool /* endOfMessage */,
            CancellationToken /* cancel */,
            Task>;

    public class OwinWebSocketWrapper : IDisposable
    {
        private readonly WebSocket _webSocket;
        private readonly IDictionary<string, object> _environment;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private WebSocketContext _context;

        public OwinWebSocketWrapper(WebSocketContext context)
        {
            _context = context;
            _webSocket = context.WebSocket;
            _cancellationTokenSource = new CancellationTokenSource();

            _environment = new Dictionary<string, object>();
            _environment[WebSocketConstants.WebSocketSendAsyncKey] = new WebSocketSendAsync(SendAsync);
            _environment[WebSocketConstants.WebSocketReceiveAyncKey] = new WebSocketReceiveAsync(ReceiveAsync);
            _environment[WebSocketConstants.WebSocketCloseAsyncKey] = new WebSocketCloseAsync(CloseAsync);
            _environment[WebSocketConstants.WebSocketCallCancelledKey] = _cancellationTokenSource.Token;
            _environment[WebSocketConstants.WebSocketVersionKey] = WebSocketConstants.WebSocketVersion;

            _environment[typeof(WebSocketContext).FullName] = context;
        }

        public IDictionary<string, object> Environment
        {
            get { return _environment; }
        }

        public Task SendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            // Remap close messages to CloseAsync.  System.Net.WebSockets.WebSocket.SendAsync does not allow close messages.
            if (messageType == 0x8)
            {
                return RedirectSendToCloseAsync(buffer, cancel);
            }
            else if (messageType == 0x9 || messageType == 0xA)
            {
                // Ping & Pong, not allowed by the underlying APIs, silently discard.
                return TaskHelpers.Completed();
            }

            return _webSocket.SendAsync(buffer, OpCodeToEnum(messageType), endOfMessage, cancel);
        }

        public async Task<WebSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            WebSocketReceiveResult nativeResult = await _webSocket.ReceiveAsync(buffer, cancel);

            if (nativeResult.MessageType == WebSocketMessageType.Close)
            {
                _environment[WebSocketConstants.WebSocketCloseStatusKey] = (int)(nativeResult.CloseStatus ?? WebSocketCloseStatus.NormalClosure);
                _environment[WebSocketConstants.WebSocketCloseDescriptionKey] = nativeResult.CloseStatusDescription ?? string.Empty;
            }

            return new WebSocketReceiveTuple(
                EnumToOpCode(nativeResult.MessageType),
                nativeResult.EndOfMessage,
                nativeResult.Count);
        }

        public Task CloseAsync(int status, string description, CancellationToken cancel)
        {
            return _webSocket.CloseOutputAsync((WebSocketCloseStatus)status, description, cancel);
        }

        private Task RedirectSendToCloseAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            if (buffer.Array == null || buffer.Count == 0)
            {
                return CloseAsync(1000, string.Empty, cancel);
            }
            else if (buffer.Count >= 2)
            {
                // Unpack the close message.
                int statusCode =
                    (buffer.Array[buffer.Offset] << 8)
                        | buffer.Array[buffer.Offset + 1];
                string description = Encoding.UTF8.GetString(buffer.Array, buffer.Offset + 2, buffer.Count - 2);

                return CloseAsync(statusCode, description, cancel);
            }
            else
            {
                throw new ArgumentOutOfRangeException("buffer");
            }
        }

        public async Task CleanupAsync()
        {
            switch (_webSocket.State)
            {
                case WebSocketState.Closed: // Closed gracefully, no action needed. 
                case WebSocketState.Aborted: // Closed abortively, no action needed.                       
                    break;
                case WebSocketState.CloseReceived:
                    // Echo what the client said, if anything.
                    await _webSocket.CloseAsync(_webSocket.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                        _webSocket.CloseStatusDescription ?? string.Empty, _cancellationTokenSource.Token);
                    break;
                case WebSocketState.Open:
                case WebSocketState.CloseSent: // No close received, abort so we don't have to drain the pipe.
                    _webSocket.Abort();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state", _webSocket.State, string.Empty);
            }
        }

        private WebSocketMessageType OpCodeToEnum(int messageType)
        {
            switch (messageType)
            {
                case 0x1:
                    return WebSocketMessageType.Text;
                case 0x2:
                    return WebSocketMessageType.Binary;
                case 0x8:
                    return WebSocketMessageType.Close;
                default:
                    throw new ArgumentOutOfRangeException("messageType", messageType, string.Empty);
            }
        }

        private int EnumToOpCode(WebSocketMessageType webSocketMessageType)
        {
            switch (webSocketMessageType)
            {
                case WebSocketMessageType.Text:
                    return 0x1;
                case WebSocketMessageType.Binary:
                    return 0x2;
                case WebSocketMessageType.Close:
                    return 0x8;
                default:
                    throw new ArgumentOutOfRangeException("webSocketMessageType", webSocketMessageType, string.Empty);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }
        }
    }
}

#endif