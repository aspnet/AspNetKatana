// <copyright file="WebSocketEcho.cs" company="Katana contributors">
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WebSockets.Owin.Samples
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task>>; // Complete
    using WebSocketCloseAsync =
        Func<int /* closeStatus */,
            string /* closeDescription */,
            CancellationToken /* cancel */,
            Task>;
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task>; // Complete
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

    // A sample application using websockets.
    public class WebSocketEcho
    {
        private readonly AppFunc _nextApp;

        public WebSocketEcho(AppFunc nextApp)
        {
            _nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            object obj;
            if (env.TryGetValue("websocket.Accept", out obj)
                && obj != null)
            {
                WebSocketAccept accept = (WebSocketAccept)obj;
                IDictionary<string, string[]> requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                Dictionary<string, object> acceptOptions = null;
                string[] subProtocols;
                if (requestHeaders.TryGetValue("Sec-WebSocket-Protocol", out subProtocols) && subProtocols.Length > 0)
                {
                    acceptOptions = new Dictionary<string, object>();
                    // Select the first one from the client
                    acceptOptions.Add("websocket.SubProtocol", subProtocols[0].Split(',').First().Trim());
                }

                accept(acceptOptions, new WebSocketFunc(DoEcho));
                return TaskHelpers.Completed();
            }
            else
            {
                return _nextApp(env);
            }
        }

        public async Task DoEcho(IDictionary<string, object> webSocketEnv)
        {
            WebSocketSendAsync sendAsync = webSocketEnv.Get<WebSocketSendAsync>("websocket.SendAsync");
            WebSocketReceiveAsync receiveAsync = webSocketEnv.Get<WebSocketReceiveAsync>("websocket.ReceiveAsync");
            WebSocketCloseAsync closeAsync = webSocketEnv.Get<WebSocketCloseAsync>("websocket.CloseAsync");
            CancellationToken cancel = webSocketEnv.Get<CancellationToken>("websocket.CallCancelled");

            byte[] buffer = new byte[1024];
            WebSocketReceiveTuple receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), cancel);

            // Echo until closed
            while (receiveResult.Item1 != 0x8)
            {
                await sendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Item3), receiveResult.Item1, receiveResult.Item2, cancel);
                receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await closeAsync(webSocketEnv.Get<int>("websocket.ClientCloseStatus", 1000),
                webSocketEnv.Get<string>("websocket.ClientCloseDescription", "Closed"), cancel);
        }
    }
}
