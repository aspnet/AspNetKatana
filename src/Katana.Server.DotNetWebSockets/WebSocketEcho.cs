using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Katana.Server.DotNetWebSockets
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

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
            if (env.TryGetValue("websocket.Support", out obj) && obj.Equals("WebSocketFunc"))
            {
                env["owin.ResponseStatusCode"] = 101;
                IDictionary<string, string[]> requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                IDictionary<string, string[]> responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");

                string[] subProtocols;
                if (requestHeaders.TryGetValue("Sec-WebSocket-Protocol", out subProtocols) && subProtocols.Length > 0)
                {
                    // Select the first one from the client
                    responseHeaders["Sec-WebSocket-Protocol"] = new string[] { subProtocols[0].Split(',').First().Trim() };
                }

                env["websocket.Func"] = new WebSocketFunc(DoEcho);
                return TaskHelpers.Completed();
            }
            else
            {
                return nextApp(env);
            }
        }

        public async Task DoEcho(IDictionary<string, object> wsEnv)
        {
            WebSocketSendAsync sendAsync = wsEnv.Get<WebSocketSendAsync>("websocket.SendAsyncFunc");
            WebSocketReceiveAsync receiveAsync = wsEnv.Get<WebSocketReceiveAsync>("websocket.ReceiveAsyncFunc");
            WebSocketCloseAsync closeAsync = wsEnv.Get<WebSocketCloseAsync>("websocket.CloseAsyncFunc");

            byte[] buffer = new byte[1024];
            WebSocketReceiveTuple receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (receiveResult.Item1 != 0x8) // Echo until closed
            {
                await sendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Item3.Value), receiveResult.Item1, receiveResult.Item2, CancellationToken.None);
                receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await closeAsync(receiveResult.Item4 ?? 1000, receiveResult.Item5 ?? "Closed", CancellationToken.None);
        }
    }
}