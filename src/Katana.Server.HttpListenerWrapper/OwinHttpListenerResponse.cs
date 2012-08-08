//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Katana.Server.HttpListenerWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Owin;
    using System.Net.WebSockets;

    #pragma warning disable 811
    using WebSocketAction =
        Func
        <
        // SendAsync
            Func
            <
                ArraySegment<byte> /* data */,
                int /* messageType */,
                bool /* endOfMessage */,
                CancellationToken /* cancel */,
                Task
            >,
        // ReceiveAsync
            Func
            <
                ArraySegment<byte> /* data */,
                CancellationToken /* cancel */,
                Task
                <
                    Tuple
                    <
                        int /* messageType */,
                        bool /* endOfMessage */,
                        int? /* count */,
                        int? /* closeStatus */,
                        string /* closeStatusDescription */
                    >
                >
            >,
        // CloseAsync
            Func
            <
                int /* closeStatus */,
                string /* closeDescription */,
                CancellationToken /* cancel */,
                Task
            >,
        // Complete
            Task
        >;

    using WSReceiveResult = Tuple
        <
            int /* messageType */,
            bool /* endOfMessage */,
            int? /* count */,
            int? /* closeStatus */,
            string /* closeStatusDescription */
        >;

    /// <summary>
    /// This wraps an HttpListenerResponse, populates it with the given response fields, and relays 
    /// the response body to the underlying stream.
    /// </summary>
    internal class OwinHttpListenerResponse
    {
        private HttpListenerContext context;
        private HttpListenerResponse response;
        private Func<Stream, Task> bodyDelegate;
        private IDictionary<string, object> properties;
        private WebSocketContext webSocketContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinHttpListenerResponse"/> class.
        /// Copies the status and headers into the response object.
        /// </summary>
        /// <param name="response">The response to copy the OWIN data into.</param>
        /// <param name="result">The status, headers, body, and properties.</param>
        public OwinHttpListenerResponse(HttpListenerContext context, ResultParameters result)
        {
            Contract.Requires(context != null);
            Contract.Requires(result.Properties != null);
            this.context = context;
            this.response = context.Response;
            this.bodyDelegate = result.Body;
            this.properties = result.Properties;

            if (result.Status == 100)
            {
                throw new ArgumentOutOfRangeException("result.Status", result.Status, string.Empty);
            }

            // Status
            this.response.StatusCode = result.Status;
            
            // Optional reason phrase
            object reasonPhrase;
            if (result.Properties != null 
                && result.Properties.TryGetValue(Constants.ReasonPhraseKey, out reasonPhrase)
                && !string.IsNullOrWhiteSpace((string)reasonPhrase))
            {
                this.response.StatusDescription = (string)reasonPhrase;
            }

            // Version, e.g. HTTP/1.1
            object httpVersion;
            if (result.Properties != null
                && result.Properties.TryGetValue(Constants.HttpResponseProtocolKey, out httpVersion)
                && !string.IsNullOrWhiteSpace((string)httpVersion))
            {
                string httpVersionString = (string)httpVersion;
                Contract.Requires(httpVersionString.StartsWith("HTTP/"));
                Version version = Version.Parse(httpVersionString.Substring(httpVersionString.IndexOf('/') + 1));
                this.response.ProtocolVersion = version;
            }

            // Headers
            if (result.Headers != null && result.Headers.Count > 0)
            {
                this.CopyResponseHeaders(result.Headers);
            }
        }

        private void CopyResponseHeaders(IDictionary<string, string[]> responseHeaders)
        {
            foreach (KeyValuePair<string, string[]> header in responseHeaders)
            {
                foreach (string value in header.Value)
                {
                    this.AddHeaderValue(header.Key, value);
                }
            }

            string[] wwwAuthValues;
            if (responseHeaders.TryGetValue(Constants.WwwAuthenticateHeader, out wwwAuthValues))
            {
                // Uses InternalAdd to bypass a response header restriction, but to do so we must merge the values.
                this.response.AddHeader(Constants.WwwAuthenticateHeader, string.Join(", ", wwwAuthValues));
            }
        }

        private void AddHeaderValue(string header, string value)
        {
            try
            {
                // Some header values are restricted
                if (header.Equals(Constants.ContentLengthHeader, StringComparison.OrdinalIgnoreCase))
                {
                    this.response.ContentLength64 = long.Parse(value);
                }
                else if (header.Equals(Constants.TransferEncodingHeader, StringComparison.OrdinalIgnoreCase)
                    && value.Equals("chunked", StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: what about a mixed format value like chunked, otherTransferEncoding?
                    this.response.SendChunked = true;
                }
                else if (header.Equals(Constants.ConnectionHeader, StringComparison.OrdinalIgnoreCase)
                    && value.Equals("close", StringComparison.OrdinalIgnoreCase))
                {
                    this.response.KeepAlive = false;
                }
                else if (header.Equals(Constants.KeepAliveHeader, StringComparison.OrdinalIgnoreCase)
                    && value.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    // HTTP/1.0 semantics
                    this.response.KeepAlive = true;
                }
                else if (header.Equals(Constants.WwwAuthenticateHeader, StringComparison.OrdinalIgnoreCase))
                {
                    // WWW-Authenticate is restricted and must use Response.AddHeader with a single 
                    // merged value.  See CopyResponseHeaders.
                }
                else
                {
                    this.response.Headers.Add(header, value);
                }
            }
            catch (Exception)
            {
                // TODO: Logging; Debug.Assert(false, "Bad response header: " + header);
                throw;
            }
        }

        // The caller will handle errors and abort the request.
        public async Task ProcessBodyAsync()
        {
            object temp;
            if (this.response.StatusCode == 101
                && this.properties != null 
                && this.properties.TryGetValue(Constants.WebSocketBodyDelegte, out temp)
                && temp != null)
            {
                WebSocketAction wsDelegate = (WebSocketAction)temp;
                this.webSocketContext = await this.context.AcceptWebSocketAsync(null); // TODO: Sub protocol
                await wsDelegate(this.WSSendAsync, this.WSReceiveAsync, this.WSCloseAsync);
                
                // Cleanup
                switch (this.webSocketContext.WebSocket.State)
                {
                    case WebSocketState.Closed: // Closed gracefully, no action needed. 
                    case WebSocketState.Aborted: // Closed abortively, no action needed.                       
                        break;
                    case WebSocketState.CloseReceived:
                        await this.webSocketContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                            string.Empty, CancellationToken.None /*TODO:*/);
                        break;
                    case WebSocketState.Open: 
                    case WebSocketState.CloseSent: // No close received, abort so we don't have to drain the pipe.
                        this.webSocketContext.WebSocket.Abort();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("state", this.webSocketContext.WebSocket.State, string.Empty);
                }
            }
            if (this.bodyDelegate == null)
            {
                this.response.Close();
            }
            else
            {
                Stream responseOutput = new HttpListenerStreamWrapper(this.response.OutputStream);
                await this.bodyDelegate(responseOutput);
                this.response.Close();
            }
        }

        private Task WSSendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            return this.webSocketContext.WebSocket.SendAsync(buffer, OpCodeToEnum(messageType), endOfMessage, cancel);
        }

        private async Task<WSReceiveResult> WSReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            WebSocketReceiveResult nativeResult = await this.webSocketContext.WebSocket.ReceiveAsync(buffer, cancel);
            return new WSReceiveResult(
                EnumToOpCode(nativeResult.MessageType), 
                nativeResult.EndOfMessage, 
                (nativeResult.MessageType == WebSocketMessageType.Close ? null : (int?)nativeResult.Count),
                (int?)nativeResult.CloseStatus,
                nativeResult.CloseStatusDescription
                );
        }

        private Task WSCloseAsync(int status, string description, CancellationToken cancel)
        {
            return this.webSocketContext.WebSocket.CloseOutputAsync((WebSocketCloseStatus)status, description, cancel);
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
