using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gate.Middleware;
using System.Threading;
using System.Threading.Tasks;

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

    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.UseShowExceptions();
            builder.UseFunc<AppDelegate>(Startup.WebSocketsApp);
        }

        private static AppDelegate WebSocketsApp(AppDelegate app)
        {
            return (call =>
            {
                ResultParameters result = new ResultParameters();
                result.Properties = new Dictionary<string, object>();
                result.Headers = new Dictionary<string, string[]>();

                object obj;
                if (call.Environment.TryGetValue("websocket.Support", out obj))
                {
                    result.Status = 101;
                    WebSocketFunc func = (send, receive, close) =>
                    {
                        return close(1000, "Done", CancellationToken.None);
                    };
                    result.Properties["websocket.Func"] = func;
                }
                else
                {
                    result.Status = 200;
                }
                return TaskHelpers.FromResult(result);
            });
        }
    }
}