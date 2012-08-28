using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gate.Middleware;
using System.Threading;
using System.Threading.Tasks;
using Gate;
using System.IO;
using System.Globalization;

namespace Katana.Server.AspNet.WebSocketsApp
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

#pragma warning disable 811
    using WebSocketFunc =
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
#pragma warning restore 811

    using WebSocketReceiveTuple = Tuple
        <
            int /* messageType */,
            bool /* endOfMessage */,
            int? /* count */,
            int? /* closeStatus */,
            string /* closeStatusDescription */
        >;

    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.UseShowExceptions();
            builder.UseFunc<AppFunc>(Startup.WebSocketsApp);
        }

        // TODO: What signature would make this a dead end app?
        private static AppFunc WebSocketsApp(AppFunc app)
        {
            return (env =>
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

                    WebSocketFunc func = async (sendAsync, receiveAsync, closeAsync) =>
                    {
                        byte[] buffer = new byte[1024];
                        WebSocketReceiveTuple receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        while (receiveResult.Item1 != 0x8) // Echo until closed
                        {
                            await sendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Item3.Value), receiveResult.Item1, receiveResult.Item2, CancellationToken.None);
                            receiveResult = await receiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        }

                        await closeAsync(receiveResult.Item4 ?? 1000, receiveResult.Item5 ?? "Closed", CancellationToken.None);
                    };
                    env["websocket.Func"] = func;
                    return TaskHelpers.Completed();
                }
                else
                {
                    return app(env);
                }
            });
        }
    }
}