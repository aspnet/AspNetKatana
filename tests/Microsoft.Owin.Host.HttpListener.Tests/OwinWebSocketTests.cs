// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Host.HttpListener.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using WebSocketAccept = Action<IDictionary<string, object>, Func<IDictionary<string, object>, Task>>;
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
                int /* count */>>>;
    using WebSocketSendAsync =
        Func<ArraySegment<byte> /* data */,
            int /* messageType */,
            bool /* endOfMessage */,
            CancellationToken /* cancel */,
            Task>;

    public class OwinWebSocketTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http", "localhost", "8080", "/BaseAddress/" };
        private const string WsClientAddress = "ws://localhost:8080/BaseAddress/";

        [Fact]
        public async Task EndToEnd_ConnectAndClose_Success()
        {
            OwinHttpListener listener = CreateServer(env =>
            {
                var accept = (WebSocketAccept)env["websocket.Accept"];
                Assert.NotNull(accept);

                accept(
                    null,
                    async wsEnv =>
                    {
                        var sendAsync1 = wsEnv.Get<WebSocketSendAsync>("websocket.SendAsync");
                        var receiveAsync1 = wsEnv.Get<WebSocketReceiveAsync>("websocket.ReceiveAsync");
                        var closeAsync1 = wsEnv.Get<WebSocketCloseAsync>("websocket.CloseAsync");

                        var buffer1 = new ArraySegment<byte>(new byte[10]);
                        await receiveAsync1(buffer1, CancellationToken.None);
                        await closeAsync1((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    });

                return Task.FromResult(0);
            },
                HttpServerAddress);

            using (listener)
            {
                using (var client = new ClientWebSocket())
                {
                    await client.ConnectAsync(new Uri(WsClientAddress), CancellationToken.None);

                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    WebSocketReceiveResult readResult = await client.ReceiveAsync(new ArraySegment<byte>(new byte[10]), CancellationToken.None);

                    Assert.Equal(WebSocketCloseStatus.NormalClosure, readResult.CloseStatus);
                    Assert.Equal("Closing", readResult.CloseStatusDescription);
                    Assert.Equal(0, readResult.Count);
                    Assert.True(readResult.EndOfMessage);
                    Assert.Equal(WebSocketMessageType.Close, readResult.MessageType);
                }
            }
        }

        [Fact]
        public async Task EndToEnd_EchoData_Success()
        {
            ManualResetEvent echoComplete = new ManualResetEvent(false);
            OwinHttpListener listener = CreateServer(env =>
            {
                var accept = (WebSocketAccept)env["websocket.Accept"];
                Assert.NotNull(accept);

                accept(
                    null,
                    async wsEnv =>
                    {
                        var sendAsync = wsEnv.Get<WebSocketSendAsync>("websocket.SendAsync");
                        var receiveAsync = wsEnv.Get<WebSocketReceiveAsync>("websocket.ReceiveAsync");
                        var closeAsync = wsEnv.Get<WebSocketCloseAsync>("websocket.CloseAsync");

                        var buffer = new ArraySegment<byte>(new byte[100]);
                        Tuple<int, bool, int> serverReceive = await receiveAsync(buffer, CancellationToken.None);
                        await sendAsync(new ArraySegment<byte>(buffer.Array, 0, serverReceive.Item3),
                            serverReceive.Item1, serverReceive.Item2, CancellationToken.None);
                        Assert.True(echoComplete.WaitOne(100), "Echo incomplete");
                        await closeAsync((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    });

                return Task.FromResult(0);
            },
                HttpServerAddress);

            using (listener)
            {
                using (var client = new ClientWebSocket())
                {
                    await client.ConnectAsync(new Uri(WsClientAddress), CancellationToken.None);

                    byte[] sendBody = Encoding.UTF8.GetBytes("Hello World");
                    await client.SendAsync(new ArraySegment<byte>(sendBody), WebSocketMessageType.Text, true, CancellationToken.None);
                    var receiveBody = new byte[100];
                    WebSocketReceiveResult readResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBody), CancellationToken.None);
                    echoComplete.Set();

                    Assert.Equal(WebSocketMessageType.Text, readResult.MessageType);
                    Assert.True(readResult.EndOfMessage);
                    Assert.Equal(sendBody.Length, readResult.Count);
                    Assert.Equal("Hello World", Encoding.UTF8.GetString(receiveBody, 0, readResult.Count));
                }
            }
        }

        [Fact]
        public async Task SubProtocol_SelectLastSubProtocol_Success()
        {
            OwinHttpListener listener = CreateServer(env =>
            {
                var accept = (WebSocketAccept)env["websocket.Accept"];
                Assert.NotNull(accept);

                var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");

                // Select the last sub-protocol from the client.
                string subProtocol = requestHeaders["Sec-WebSocket-Protocol"].Last().Split(',').Last().Trim();

                responseHeaders["Sec-WebSocket-Protocol"] = new string[] { subProtocol + "A" };

                accept(
                    new Dictionary<string, object>() { { "websocket.SubProtocol", subProtocol } },
                    async wsEnv =>
                    {
                        var sendAsync = wsEnv.Get<WebSocketSendAsync>("websocket.SendAsync");
                        var receiveAsync = wsEnv.Get<WebSocketReceiveAsync>("websocket.ReceiveAsync");
                        var closeAsync = wsEnv.Get<WebSocketCloseAsync>("websocket.CloseAsync");

                        var buffer = new ArraySegment<byte>(new byte[100]);
                        Tuple<int, bool, int> serverReceive = await receiveAsync(buffer, CancellationToken.None);
                        // Assume close received
                        await closeAsync((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    });

                return Task.FromResult(0);
            },
                HttpServerAddress);

            using (listener)
            {
                using (var client = new ClientWebSocket())
                {
                    client.Options.AddSubProtocol("protocol1");
                    client.Options.AddSubProtocol("protocol2");

                    await client.ConnectAsync(new Uri(WsClientAddress), CancellationToken.None);

                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    var receiveBody = new byte[100];
                    WebSocketReceiveResult readResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBody), CancellationToken.None);
                    Assert.Equal(WebSocketMessageType.Close, readResult.MessageType);
                    Assert.Equal("protocol2", client.SubProtocol);
                }
            }
        }

        [Fact]
        public async Task WebSocketUpgradeAfterHeadersSent_Throws()
        {
            ManualResetEvent sync = new ManualResetEvent(false);
            OwinHttpListener listener = CreateServer(env =>
                {
                    var accept = (WebSocketAccept)env["websocket.Accept"];
                    Assert.NotNull(accept);

                    // Send a response
                    Stream responseStream = (Stream)env["owin.ResponseBody"];
                    responseStream.Write(new byte[100], 0, 100);
                    sync.Set();

                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        accept(
                            null,
                            async wsEnv =>
                            {
                                throw new Exception("This wasn't supposed to get called.");
                            });
                    });

                    sync.Set();

                    return Task.FromResult(0);
                },
                HttpServerAddress);

            using (listener)
            {
                using (var client = new ClientWebSocket())
                {
                    try
                    {
                        Task task = client.ConnectAsync(new Uri(WsClientAddress), CancellationToken.None);
                        Assert.True(sync.WaitOne(500));
                        await task;
                        Assert.Equal(string.Empty, "A WebSocketException was expected.");
                    }
                    catch (WebSocketException)
                    {
                    }
                }
            }
        }

        [Fact]
        public async Task ErrorInWebSocket_Disconnected()
        {
            ManualResetEvent sync = new ManualResetEvent(false);
            OwinHttpListener listener = CreateServer(env =>
                {
                    var accept = (WebSocketAccept)env["websocket.Accept"];
                    Assert.NotNull(accept);

                    accept(
                        null,
                        async wsEnv =>
                        {
                            sync.Set();
                            throw new Exception("Application WebSocket error.");
                        });

                    return Task.FromResult(0);
                },
                HttpServerAddress);

            using (listener)
            {
                using (var client = new ClientWebSocket())
                {
                    await client.ConnectAsync(new Uri(WsClientAddress), CancellationToken.None);

                    try
                    {
                        Assert.True(sync.WaitOne(500));
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        Assert.Equal(string.Empty, "A WebSocketException was expected.");
                    }
                    catch (WebSocketException)
                    {
                    }
                }
            }
        }

        private OwinHttpListener CreateServer(AppFunc app, string[] addressParts)
        {
            var wrapper = new OwinHttpListener();
            wrapper.Start(wrapper.Listener, app, CreateAddress(addressParts), null, null);
            return wrapper;
        }

        private static IList<IDictionary<string, object>> CreateAddress(string[] addressParts)
        {
            var address = new Dictionary<string, object>();
            address["scheme"] = addressParts[0];
            address["host"] = addressParts[1];
            address["port"] = addressParts[2];
            address["path"] = addressParts[3];

            IList<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            list.Add(address);
            return list;
        }
    }
}
