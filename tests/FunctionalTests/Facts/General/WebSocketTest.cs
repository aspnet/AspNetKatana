// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.General
{
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

    public class WebSocketTest
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public async Task WebsocketBasic(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, WebsocketBasicConfiguration);

                using (var client = new ClientWebSocket())
                {
                    await client.ConnectAsync(new Uri(applicationUrl.Replace("http://", "ws://")), CancellationToken.None);

                    var receiveBody = new byte[100];

                    for (int i = 0; i < 4; i++)
                    {
                        var message = "Message " + i.ToString();
                        byte[] sendBody = Encoding.UTF8.GetBytes(message);
                        await client.SendAsync(new ArraySegment<byte>(sendBody), WebSocketMessageType.Text, true, CancellationToken.None);
                        var receiveResult = await client.ReceiveAsync(new ArraySegment<byte>(receiveBody), CancellationToken.None);

                        Assert.Equal(WebSocketMessageType.Text, receiveResult.MessageType);
                        Assert.True(receiveResult.EndOfMessage);
                        Assert.Equal(sendBody.Length, receiveResult.Count);
                        Assert.Equal(message, Encoding.UTF8.GetString(receiveBody, 0, receiveResult.Count));
                    }
                }
            }
        }

        public void WebsocketBasicConfiguration(IAppBuilder app)
        {
            app.Run(async context =>
                {
                    var acceptWebsocketConnection = context.Get<WebSocketAccept>("websocket.Accept");

                    if (acceptWebsocketConnection != null)
                    {
                        var acceptDictionary = new Dictionary<string, object>();
                        acceptDictionary.Add("websocket.ReceiveBufferSize", 300);
                        acceptDictionary.Add("websocket.KeepAliveInterval", TimeSpan.FromMinutes(50));
                        var websocketBuffer = new ArraySegment<byte>(new byte[1000]);
                        acceptDictionary.Add("websocket.Buffer", websocketBuffer);

                        acceptWebsocketConnection(acceptDictionary, async websocketEnvironment =>
                            {
                                var sendAsync = websocketEnvironment.Get<WebSocketSendAsync>("websocket.SendAsync");
                                var receiveAsync = websocketEnvironment.Get<WebSocketReceiveAsync>("websocket.ReceiveAsync");
                                var closeAsync = websocketEnvironment.Get<WebSocketCloseAsync>("websocket.CloseAsync");

                                for (int i = 0; i < 4; i++)
                                {
                                    //Receive data
                                    var receiveBuffer = new ArraySegment<byte>(new byte[100]);
                                    Tuple<int, bool, int> serverReceive = await receiveAsync(receiveBuffer, CancellationToken.None);

                                    Assert.True(websocketBuffer.Array.Any(b => (b != 0)), "The user provided buffer does not seem to be utilized");
                                    Assert.Equal("Message " + i.ToString(), Encoding.UTF8.GetString(receiveBuffer.Array, 0, receiveBuffer.Count).TrimEnd('\0'));

                                    //Echo same data back
                                    await sendAsync(new ArraySegment<byte>(receiveBuffer.Array, 0, serverReceive.Item3), serverReceive.Item1, serverReceive.Item2, CancellationToken.None);
                                }

                                //Close the connection
                                await closeAsync((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            });
                    }
                    else
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Not a web socket connection");
                    }
                });
        }
    }
}