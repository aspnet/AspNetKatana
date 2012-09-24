using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WebSockets.Owin.Samples
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    using WebSocketAccept =
        Action
        <
            IDictionary<string, object>, // WebSocket Accept parameters
            Func // WebSocketFunc callback
            <
                IDictionary<string, object>, // WebSocket environment
                Task // Complete
            >
        >;

    using WebSocketFunc =
        Func
        <
            IDictionary<string, object>, // WebSocket environment
            Task // Complete
        >;

    using WebSocketSendAsync =
        Func
        <
            ArraySegment<byte> /* data */,
            int /* messageType */,
            bool /* endOfMessage */,
            CancellationToken /* cancel */,
            Task
        >;

    using WebSocketReceiveAsync =
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
        >;

    using WebSocketReceiveTuple =
        Tuple
        <
            int /* messageType */,
            bool /* endOfMessage */,
            int? /* count */,
            int? /* closeStatus */,
            string /* closeStatusDescription */
        >;

    using WebSocketCloseAsync =
        Func
        <
            int /* closeStatus */,
            string /* closeDescription */,
            CancellationToken /* cancel */,
            Task
        >;

    // A sample application using websockets.
    public class WebSocketEcho
    {
        private AppFunc nextApp;

        public WebSocketEcho(AppFunc nextApp)
        {
            this.nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            object obj;
            if (env.TryGetValue("websocket.Support", out obj) 
                && "websocket.Accept".Equals(obj)
                && env.TryGetValue("websocket.Accept", out obj)
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
                return nextApp(env);
            }
        }

        public async Task DoEcho(IDictionary<string, object> wsEnv)
        {
            WebSocketSendAsync sendAsync = wsEnv.Get<WebSocketSendAsync>("websocket.SendAsync");
            WebSocketReceiveAsync receiveAsync = wsEnv.Get<WebSocketReceiveAsync>("websocket.ReceiveAsync");
            WebSocketCloseAsync closeAsync = wsEnv.Get<WebSocketCloseAsync>("websocket.CloseAsync");
            CancellationToken cancel = wsEnv.Get<CancellationToken>("websocket.CallCancelled");

            byte[] buffer = new byte[1024];
            WebSocketReceiveTuple receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), cancel);

            while (receiveResult.Item1 != 0x8) // Echo until closed
            {
                await sendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Item3.Value), receiveResult.Item1, receiveResult.Item2, cancel);
                receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await closeAsync(receiveResult.Item4 ?? 1000, receiveResult.Item5 ?? "Closed", cancel);
        }
    }
}