using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 1998

namespace Katana.Performance.ReferenceApp
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
                    int /* count */
                >
            >
        >;

    public class CanonicalRequestPatterns
    {
        private readonly AppFunc _next;
        private readonly Dictionary<string, Tuple<AppFunc, string>> _paths;

        private readonly byte[] _2KAlphabet = Util.AlphabetCRLF(2 << 10).ToArray();

        public CanonicalRequestPatterns(AppFunc next)
        {
            _next = next;

            _paths = new Dictionary<string, Tuple<AppFunc, string>>();
            _paths["/"] = new Tuple<AppFunc, string>(Index, null);

            var items = GetType().GetMethods()
                .Select(methodInfo => new
                {
                    MethodInfo = methodInfo,
                    Attribute = methodInfo.GetCustomAttributes(true).OfType<CanonicalRequestAttribute>().SingleOrDefault()
                })
                .Where(item => item.Attribute != null)
                .Select(item => new
                {
                    App = (AppFunc)Delegate.CreateDelegate(typeof(AppFunc), this, item.MethodInfo),
                    item.Attribute.Description,
                    item.Attribute.Path,
                });

            foreach (var item in items)
            {
                _paths.Add(item.Path, Tuple.Create(item.App, item.Description));
            }
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            Tuple<AppFunc, string> handler;
            return _paths.TryGetValue(Util.RequestPath(env), out handler)
                ? handler.Item1(env)
                : _next(env);
        }


        public async Task Index(IDictionary<string, object> env)
        {
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/html" };
            var output = Util.ResponseBody(env);
            using (var writer = new StreamWriter(output))
            {
                writer.Write("<ul>");
                foreach (var kv in _paths.Where(item => item.Value.Item2 != null))
                {
                    writer.Write("<li><a href='");
                    writer.Write(kv.Key);
                    writer.Write("'>");
                    writer.Write(kv.Key);
                    writer.Write("</a> ");
                    writer.Write(kv.Value.Item2);
                    writer.Write("</li>");
                }
                writer.Write("</ul>");
            }
        }


        [CanonicalRequest(Path = "/small-immediate-syncwrite", Description = "Return 2kb ascii byte[] in a sync Write")]
        public async Task SmallImmediateSyncWrite(IDictionary<string, object> env)
        {
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            Util.ResponseBody(env).Write(_2KAlphabet, 0, 2048);
        }

        [CanonicalRequest(Path = "/large-immediate-syncwrite", Description = "Return 1mb ascii byte[] in 2kb sync Write")]
        public async Task LargeImmediateSyncWrite(IDictionary<string, object> env)
        {
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = Util.ResponseBody(env);
            for (var loop = 0; loop != (1 << 20) / (2 << 10); ++loop)
            {
                responseBody.Write(_2KAlphabet, 0, 2048);
            }
        }

        [CanonicalRequest(Path = "/large-immediate-asyncwrite", Description = "Return 1mb ascii byte[] in 2kb await WriteAsync")]
        public async Task LargeImmediateAsyncWrite(IDictionary<string, object> env)
        {
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = Util.ResponseBody(env);
            for (var loop = 0; loop != (1 << 20) / (2 << 10); ++loop)
            {
                await responseBody.WriteAsync(_2KAlphabet, 0, 2048);
            }
        }

        [CanonicalRequest(Path = "/large-blockingwork-syncwrite", Description = "Return 1mb ascii byte[] in 2kb sync Write with 20ms thread sleeps every 8 writes")]
        public async Task LargeBlockingWorkSyncWrite(IDictionary<string, object> env)
        {
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = Util.ResponseBody(env);
            for (var loop = 0; loop != (1 << 20) / (2 << 10); ++loop)
            {
                responseBody.Write(_2KAlphabet, 0, 2048);
                if ((loop % 8) == 0)
                {
                    Thread.Sleep(20);
                }
            }
        }

        [CanonicalRequest(Path = "/large-awaitingwork-asyncwrite", Description = "Return 1mb ascii byte[] in 2kb await WriteAsync with 20ms awaits every 8 writes")]
        public async Task LargeAwaitingWorkAsyncWrite(IDictionary<string, object> env)
        {
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = Util.ResponseBody(env);
            for (var loop = 0; loop != (1 << 20) / (2 << 10); ++loop)
            {
                await responseBody.WriteAsync(_2KAlphabet, 0, 2048);
                if ((loop % 8) == 0)
                {
                    await Task.Delay(20);
                }
            }
        }

        [CanonicalRequest(Path = "/small-longpolling-syncwrite", Description = "Return 2kb sync Write after 12sec await delay")]
        public async Task SmallLongPollingSyncWrite(IDictionary<string, object> env)
        {
            await Task.Delay(TimeSpan.FromSeconds(12));
            Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            Util.ResponseBody(env).Write(_2KAlphabet, 0, 2048);
        }

        [CanonicalRequest(Path = "/echo-websocket", Description = "Websocket accept that echoes incoming message back as outgoing")]
        public async Task EchoWebsocket(IDictionary<string, object> env)
        {
            var accept = Util.Get<WebSocketAccept>(env, "websocket.Accept");
            if (accept != null)
            {
                accept(null, EchoWebsocketCallback);
            }
            else
            {
                Util.ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
                using(var writer = new StreamWriter(Util.ResponseBody(env)))
                {
                    writer.WriteLine("This url is designed to be called with a websocket client.");
                    writer.WriteLine("It will echo incoming message data back as outgoing.");
                }
            }
        }

        private async Task EchoWebsocketCallback(IDictionary<string, object> env)
        {
            var callCancelled = Util.Get<CancellationToken>(env, "owin.CallCancelled");
            var receiveAsync = Util.Get<WebSocketReceiveAsync>(env, "websocket.ReceiveAsync");
            var sendAsync = Util.Get<WebSocketSendAsync>(env, "websocket.SendAsync");

            var buffer = new ArraySegment<byte>(new byte[2 << 10]);
            while (!callCancelled.IsCancellationRequested)
            {
                var message = await receiveAsync(buffer, callCancelled);
                await sendAsync(new ArraySegment<byte>(buffer.Array, 0, message.Item3), message.Item1, message.Item2, callCancelled);
            }
        }
    }
}
