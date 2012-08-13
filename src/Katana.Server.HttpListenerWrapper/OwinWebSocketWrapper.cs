using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Katana.Server.HttpListenerWrapper
{
    using WebSocketReceiveTuple = Tuple
        <
            int /* messageType */,
            bool /* endOfMessage */,
            int? /* count */,
            int? /* closeStatus */,
            string /* closeStatusDescription */
        >;

    public class OwinWebSocketWrapper
    {
        private WebSocketContext context;
        private WebSocket webSocket;

        public OwinWebSocketWrapper(WebSocketContext context)
        {
            this.context = context;
            this.webSocket = context.WebSocket;
        }

        public Task SendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            return this.webSocket.SendAsync(buffer, OpCodeToEnum(messageType), endOfMessage, cancel);
        }

        public async Task<WebSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            WebSocketReceiveResult nativeResult = await this.webSocket.ReceiveAsync(buffer, cancel);
            return new WebSocketReceiveTuple(
                EnumToOpCode(nativeResult.MessageType),
                nativeResult.EndOfMessage,
                (nativeResult.MessageType == WebSocketMessageType.Close ? null : (int?)nativeResult.Count),
                (int?)nativeResult.CloseStatus,
                nativeResult.CloseStatusDescription
                );
        }

        public Task CloseAsync(int status, string description, CancellationToken cancel)
        {
            return this.webSocket.CloseOutputAsync((WebSocketCloseStatus)status, description, cancel);
        }

        public async Task CleanupAsync()
        {
            switch (this.webSocket.State)
            {
                case WebSocketState.Closed: // Closed gracefully, no action needed. 
                case WebSocketState.Aborted: // Closed abortively, no action needed.                       
                    break;
                case WebSocketState.CloseReceived:
                    await this.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                        string.Empty, CancellationToken.None /*TODO:*/);
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
