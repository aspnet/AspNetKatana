//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WebSockets.Owin
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

    public class OwinWebSocketWrapper
    {
        private WebSocketContext context;
        private WebSocket webSocket;
        private IDictionary<string, object> environment;
        private CancellationToken cancellationToken;

        public OwinWebSocketWrapper(WebSocketContext context, CancellationToken ct)
        {
            this.context = context;
            this.webSocket = context.WebSocket;
            this.cancellationToken = ct;

            environment = new Dictionary<string, object>();
            environment[Constants.WebSocketSendAsyncKey] = new WebSocketSendAsync(SendAsync);
            environment[Constants.WebSocketReceiveAyncKey] = new WebSocketReceiveAsync(ReceiveAsync);
            environment[Constants.WebSocketCloseAsyncKey] = new WebSocketCloseAsync(CloseAsync);
            environment[Constants.WebSocketCallCancelledKey] = ct;
            environment[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
            
            environment[typeof(WebSocketContext).FullName] = context;
        }

        public IDictionary<string, object> Environment
        {
            get { return environment; }
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

            return this.webSocket.SendAsync(buffer, OpCodeToEnum(messageType), endOfMessage, cancel);
        }

        public async Task<WebSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            WebSocketReceiveResult nativeResult = await this.webSocket.ReceiveAsync(buffer, cancel);

            if (nativeResult.MessageType == WebSocketMessageType.Close)
            {
                environment[Constants.WebSocketCloseStatusKey] = (int)(nativeResult.CloseStatus ?? WebSocketCloseStatus.NormalClosure);
                environment[Constants.WebSocketCloseDescriptionKey] = nativeResult.CloseStatusDescription ?? string.Empty;
            }

            return new WebSocketReceiveTuple(
                EnumToOpCode(nativeResult.MessageType),
                nativeResult.EndOfMessage,
                nativeResult.Count);
        }

        public Task CloseAsync(int status, string description, CancellationToken cancel)
        {
            return this.webSocket.CloseOutputAsync((WebSocketCloseStatus)status, description, cancel);
        }

        private Task RedirectSendToCloseAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            if (buffer.Array == null || buffer.Count == 0)
            {
                return this.CloseAsync(1000, string.Empty, cancel);
            }
            else if (buffer.Count >= 2)
            {
                // Unpack the close message.
                int statusCode =
                    (buffer.Array[buffer.Offset] << 8)
                    | buffer.Array[buffer.Offset + 1];
                string description = Encoding.UTF8.GetString(buffer.Array, buffer.Offset + 2, buffer.Count - 2);

                return this.CloseAsync(statusCode, description, cancel);
            }
            else
            {
                throw new ArgumentOutOfRangeException("buffer");
            }
        }

        public async Task CleanupAsync()
        {
            switch (this.webSocket.State)
            {
                case WebSocketState.Closed: // Closed gracefully, no action needed. 
                case WebSocketState.Aborted: // Closed abortively, no action needed.                       
                    break;
                case WebSocketState.CloseReceived:
                    // Echo what the client said, if anything.
                    await this.webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.NormalClosure, 
                        webSocket.CloseStatusDescription ?? string.Empty, cancellationToken);
                    break;
                case WebSocketState.Open:
                case WebSocketState.CloseSent: // No close received, abort so we don't have to drain the pipe.
                    this.webSocket.Abort();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state", this.webSocket.State, string.Empty);
            }
        }

        private WebSocketMessageType OpCodeToEnum(int messageType)
        {
            switch (messageType)
            {
                case 0x1: return WebSocketMessageType.Text;
                case 0x2: return WebSocketMessageType.Binary;
                case 0x8: return WebSocketMessageType.Close;
                default:
                    throw new ArgumentOutOfRangeException("messageType", messageType, string.Empty);
            }
        }

        private int EnumToOpCode(WebSocketMessageType webSocketMessageType)
        {
            switch (webSocketMessageType)
            {
                case WebSocketMessageType.Text: return 0x1;
                case WebSocketMessageType.Binary: return 0x2;
                case WebSocketMessageType.Close: return 0x8;
                default:
                    throw new ArgumentOutOfRangeException("webSocketMessageType", webSocketMessageType, string.Empty);
            }
        }
    }
}
