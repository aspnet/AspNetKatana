// -----------------------------------------------------------------------
// <copyright file="WebSocketLayer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.WebSockets
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
                        int>>>; /* count */
    using WebSocketReceiveTuple =
            Tuple<int /* messageType */,
                bool /* endOfMessage */,
                int>; /* count */
    using WebSocketSendAsync =
            Func<ArraySegment<byte> /* data */,
                int /* messageType */,
                bool /* endOfMessage */,
                CancellationToken /* cancel */,
                Task>;

    // This class implements the WebSocket layer on top of an opaque stream.
    // WebSocket Extension v0.4 is currently implemented.
    public class WebSocketLayer
    {
        private readonly IDictionary<string, object> _environment;

        private Stream _incoming;
        private Stream _outgoing;

        public WebSocketLayer(IDictionary<string, object> opaqueEnv)
        {
            this._environment = opaqueEnv;
            this._environment["websocket.SendAsync"] = new WebSocketSendAsync(SendAsync);
            this._environment["websocket.ReceiveAsync"] = new WebSocketReceiveAsync(ReceiveAsync);
            this._environment["websocket.CloseAsync"] = new WebSocketCloseAsync(CloseAsync);
            this._environment["websocket.CallCancelled"] = this._environment["opaque.CallCancelled"];
            this._environment["websocket.Version"] = "1.0";

            this._incoming = this._environment.Get<Stream>("opaque.Incoming");
            this._outgoing = this._environment.Get<Stream>("opaque.Outgoing");
        }

        public IDictionary<string, object> Environment
        {
            get { return _environment; }
        }

        // Add framing and send the data.  One frame per call to Send.
        public Task SendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        // Receive frames, unmask them.
        // Should handle pings/pongs internally.
        // Should parse out Close frames.
        public Task<WebSocketReceiveTuple> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }

        // Send a close frame.  The WebSocket is not actually considered closed until a close frame has been both sent and received.
        public Task CloseAsync(int status, string description, CancellationToken cancel)
        {
            // This could just be a wrapper around SendAsync, or at least they could share an internal helper send method.
            throw new NotImplementedException();
        }

        // Shutting down.  Send a close frame if one has been received but not set. Otherwise abort (fail the Task).
        public Task CleanupAsync()
        {
            throw new NotImplementedException();
        }
    }
}
