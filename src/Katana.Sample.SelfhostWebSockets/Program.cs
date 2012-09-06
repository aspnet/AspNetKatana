using Gate.Middleware;
using Katana.Engine;
using Katana.Engine.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using System.Net.WebSockets;

namespace Katana.Sample.SelfhostWebSockets
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
    using Katana.Server.DotNetWebSockets;
    
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
            builder.Use(typeof(WebSocketEcho));
        }

        private static async void RunSampleClient()
        {
            try
            {
                ClientWebSocket webSocket = new ClientWebSocket();
                webSocket.Options.AddSubProtocol("Echo");
                webSocket.Options.AddSubProtocol("Chat");

                Console.WriteLine("Connecting");
                await webSocket.ConnectAsync(new Uri("ws://localhost:8080/"), CancellationToken.None);
                Console.WriteLine("Connected, sub-protocol: " + webSocket.SubProtocol);

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
