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
            builder.UseFunc<AppDelegate>(Startup.WebSocketsApp);
        }

        // TODO: What signature would make this a dead end app?
        private static AppDelegate WebSocketsApp(AppDelegate app)
        {
            return (call =>
            {
                ResultParameters result = new ResultParameters();
                result.Properties = new Dictionary<string, object>();
                result.Headers = new Dictionary<string, string[]>();

                object obj;
                if (call.Environment.TryGetValue("websocket.Support", out obj) && obj.Equals("WebSocketFunc"))
                {
                    result.Status = 101;

                    string[] subProtocols;
                    if (call.Headers.TryGetValue("Sec-WebSocket-Protocol", out subProtocols) && subProtocols.Length > 0)
                    {
                        // Select the first one from the client
                        result.Headers["Sec-WebSocket-Protocol"] = new string[] { subProtocols[0].Split(',').First().Trim() };
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
                    result.Properties["websocket.Func"] = func;
                }
                else
                {
                    return app(call);
                }

                return Task.FromResult(result);
            });
        }
    }
}