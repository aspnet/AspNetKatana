using Gate.Middleware;
using Katana.Engine;
using Katana.Engine.Settings;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Katana.Sample.SelfhostWebSockets
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
    using System.Net.WebSockets;

    class Program
    {
        // Use this project to F5 test different applications and servers together.
        public static void Main(string[] args)
        {
            var settings = new KatanaSettings();

            KatanaEngine engine = new KatanaEngine(settings);

            var info = new StartInfo
            {
                Server = "HttpListener", // Katana.Server.HttpListener
                Startup = "Katana.Sample.SelfhostWebSockets.Program.Configuration", // Application
                Url = "http://+:8080/",
                /*
                OutputFile = string.Empty,
                Scheme = arguments.Scheme,
                Host = arguments.Host,
                Port = arguments.Port,
                Path = arguments.Path,
                 */
            };

            IDisposable server = engine.Start(info);

            Console.WriteLine("Running, press any key to exit");

            RunSampleClient();

            Console.ReadKey();
        }

        public void Configuration(IAppBuilder builder)
        {
            var addresses = builder.Properties.Get<IList<IDictionary<string, object>>>("host.Addresses");
            addresses.Add(new Dictionary<string, object>
            {
                {"scheme", "http"},
                {"host", "*"},
                {"port", "8081"},
                {"path", "/hello"},
            });

            builder.UseShowExceptions();
            builder.UseFunc<AppDelegate>(Program.WebSocketsApp);
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

        private static async void RunSampleClient()
        {
            try
            {
                ClientWebSocket webSocket = new ClientWebSocket();
                Console.WriteLine("Connecting");
                await webSocket.ConnectAsync(new Uri("ws://localhost:8080/"), CancellationToken.None);
                Console.WriteLine("Connected");

                string message = "Hello World";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                byte[] buffer = new byte[1024];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                Console.WriteLine("MessageType: " + result.MessageType);
                Console.WriteLine("Echo: " + Encoding.UTF8.GetString(buffer, 0, result.Count));

                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "My Close Message", CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                Console.WriteLine("MessageType: " + result.MessageType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
