// <copyright file="WebSocketLayer.cs" company="Katana contributors">
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
            _environment = opaqueEnv;
            _environment["websocket.SendAsync"] = new WebSocketSendAsync(SendAsync);
            _environment["websocket.ReceiveAsync"] = new WebSocketReceiveAsync(ReceiveAsync);
            _environment["websocket.CloseAsync"] = new WebSocketCloseAsync(CloseAsync);
            _environment["websocket.CallCancelled"] = _environment["opaque.CallCancelled"];
            _environment["websocket.Version"] = "1.0";

            _incoming = _environment.Get<Stream>("opaque.Incoming");
            _outgoing = _environment.Get<Stream>("opaque.Outgoing");
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
