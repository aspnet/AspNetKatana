// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener.WebSockets
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

    internal class OwinWebSocketWrapper
    {
        private readonly WebSocket _webSocket;
        private readonly IDictionary<string, object> _environment;
        private readonly CancellationToken _cancellationToken;
        private readonly WebSocketContext _context;

        internal OwinWebSocketWrapper(WebSocketContext context, CancellationToken ct)
        {
            _context = context;
            _webSocket = _context.WebSocket;
            _cancellationToken = ct;

            _environment = new Dictionary<string, object>();
            _environment[Constants.WebSocketSendAsyncKey] = new WebSocketSendAsync(SendAsync);
            _environment[Constants.WebSocketReceiveAyncKey] = new WebSocketReceiveAsync(ReceiveAsync);
            _environment[Constants.WebSocketCloseAsyncKey] = new WebSocketCloseAsync(CloseAsync);
            _environment[Constants.WebSocketCallCancelledKey] = ct;
            _environment[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;

            _environment[typeof(WebSocketContext).FullName] = _context;
        }

        internal IDictionary<string, object> Environment
        {
            get { return _environment; }
        }

        internal Task SendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
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

        internal async Task<WebSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            WebSocketReceiveResult nativeResult = await _webSocket.ReceiveAsync(buffer, cancel);

            if (nativeResult.MessageType == WebSocketMessageType.Close)
            {
                _environment[Constants.WebSocketCloseStatusKey] = (int)(nativeResult.CloseStatus ?? WebSocketCloseStatus.NormalClosure);
                _environment[Constants.WebSocketCloseDescriptionKey] = nativeResult.CloseStatusDescription ?? string.Empty;
            }

            return new WebSocketReceiveTuple(
                EnumToOpCode(nativeResult.MessageType),
                nativeResult.EndOfMessage,
                nativeResult.Count);
        }

        internal Task CloseAsync(int status, string description, CancellationToken cancel)
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

        internal async Task CleanupAsync()
        {
            switch (_webSocket.State)
            {
                case WebSocketState.Closed: // Closed gracefully, no action needed. 
                case WebSocketState.Aborted: // Closed abortively, no action needed.                       
                    break;
                case WebSocketState.CloseReceived:
                    // Echo what the client said, if anything.
                    await _webSocket.CloseAsync(_webSocket.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                        _webSocket.CloseStatusDescription ?? string.Empty, _cancellationToken);
                    break;
                case WebSocketState.Open:
                case WebSocketState.CloseSent: // No close received, abort so we don't have to drain the pipe.
                    _webSocket.Abort();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state", _webSocket.State, string.Empty);
            }
        }

        private static WebSocketMessageType OpCodeToEnum(int messageType)
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

        private static int EnumToOpCode(WebSocketMessageType webSocketMessageType)
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
    }
}

#endif
