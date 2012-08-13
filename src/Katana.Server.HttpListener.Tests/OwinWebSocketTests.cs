using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Katana.Server.HttpListenerWrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;

namespace Katana.Server.HttpListener.Tests
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

    [TestClass]
    public class OwinWebSocketTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http://*:8080/BaseAddress/" };
        private const string WsClientAddress = "ws://localhost:8080/BaseAddress/";
        private static readonly string[] HttpsServerAddress = new string[] { "https://*:9090/BaseAddress/" };
        private const string WssClientAddress = "wss://localhost:9090/BaseAddress/";

        [TestMethod]
        public async Task EndToEnd_ConnectAndClose_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    string support = (string)call.Environment["websocket.Support"];
                    Assert.IsTrue(support == "WebSocketFunc");
                    WebSocketFunc body = async (sendAsync, receiveAsync, closeAsync) =>
                        {
                            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[10]);
                            await receiveAsync(buffer, CancellationToken.None);
                            await closeAsync((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        };

                    ResultParameters result = CreateEmptyResponse(101);
                    result.Properties.Add("websocket.Func", body);

                    return Task.FromResult(result);
                },
                HttpServerAddress);

            using (listener)
            {
                listener.Start();
                using (ClientWebSocket client = new ClientWebSocket())
                {
                    await client.ConnectAsync(new Uri(WsClientAddress), CancellationToken.None);

                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    WebSocketReceiveResult readResult = await client.ReceiveAsync(new ArraySegment<byte>(new byte[10]), CancellationToken.None);

                    Assert.AreEqual(WebSocketCloseStatus.NormalClosure, readResult.CloseStatus);
                    Assert.AreEqual("Closing", readResult.CloseStatusDescription);
                    Assert.AreEqual(0, readResult.Count);
                    Assert.IsTrue(readResult.EndOfMessage);
                    Assert.AreEqual(WebSocketMessageType.Close, readResult.MessageType);
                }
            }
        }

        [TestMethod]
        public async Task EndToEnd_EchoData_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    WebSocketFunc body =
                        async (sendAsync, receiveAsync, closeAsync) =>
                        {
                            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[100]);
                            var serverReceive = await receiveAsync(buffer, CancellationToken.None);
                            await sendAsync(new ArraySegment<byte>(buffer.Array, 0, serverReceive.Item3.Value),
                                serverReceive.Item1, serverReceive.Item2, CancellationToken.None);
                            await closeAsync((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        };

                    ResultParameters result = CreateEmptyResponse(101);
                    result.Properties.Add("websocket.Func", body);

                    return Task.FromResult(result);
                },
                HttpServerAddress);

            using (listener)
            {
                listener.Start();
                using (ClientWebSocket client = new ClientWebSocket())
                {
                    await client.ConnectAsync(new Uri(WsClientAddress), CancellationToken.None);

                    byte[] sendBody = Encoding.UTF8.GetBytes("Hello World");
                    await client.SendAsync(new ArraySegment<byte>(sendBody), WebSocketMessageType.Text, true, CancellationToken.None);
                    byte[] receiveBody = new byte[100];
                    WebSocketReceiveResult readResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBody), CancellationToken.None);

                    Assert.AreEqual(WebSocketMessageType.Text, readResult.MessageType);
                    Assert.IsTrue(readResult.EndOfMessage);
                    Assert.AreEqual(sendBody.Length, readResult.Count);
                    Assert.AreEqual("Hello World", Encoding.UTF8.GetString(receiveBody, 0, readResult.Count));
                }
            }
        }

        private ResultParameters CreateEmptyResponse(int statusCode)
        {
            return new ResultParameters()
            {
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
                Status = statusCode,
                Properties = new Dictionary<string, object>(StringComparer.Ordinal),
                Body = null
            };
        }
    }
}
