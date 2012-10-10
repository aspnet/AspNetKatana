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

    public class CanonicalRequestPatterns
    {
        private readonly AppFunc _next;
        private readonly Dictionary<string, Tuple<AppFunc, string>> _paths;

        private readonly byte[] _2KAlphabet = AlphabetCRLF(2 << 10).ToArray();

        private static IEnumerable<byte> AlphabetCRLF(int length)
        {
            while (true)
            {
                for (var ch = 'a'; ch != 'z' + 1; ++ch)
                {
                    if (length-- == 0)
                    {
                        yield break;
                    }
                    yield return (byte)ch;
                }
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)' ';
                for (var ch = 'A'; ch != 'Z' + 1; ++ch)
                {
                    if (length-- == 0)
                    {
                        yield break;
                    }
                    yield return (byte)ch;
                }
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)'\r';
                if (length-- == 0)
                {
                    yield break;
                }
                yield return (byte)'\n';
            }
        }


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
            if (_paths.TryGetValue(Get<String>(env, "owin.RequestPath"), out handler))
                return handler.Item1(env);

            return _next(env);
        }

        T Get<T>(IDictionary<string, object> env, string key) where T : class
        {
            object value;
            return env.TryGetValue(key, out value) ? value as T : default(T);
        }

        private Stream ResponseBody(IDictionary<string, object> env)
        {
            return Get<Stream>(env, "owin.ResponseBody");
        }

        private IDictionary<string, string[]> ResponseHeaders(IDictionary<string, object> env)
        {
            return Get<IDictionary<string, string[]>>(env, "owin.ResponseHeaders");
        }

        public async Task Index(IDictionary<string, object> env)
        {
            ResponseHeaders(env)["Content-Type"] = new[] { "text/html" };
            var output = ResponseBody(env);
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
            ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            ResponseBody(env).Write(_2KAlphabet, 0, 2048);
        }

        [CanonicalRequest(Path = "/large-immediate-syncwrite", Description = "Return 1mb ascii byte[] in 2kb sync Write")]
        public async Task LargeImmediateSyncWrite(IDictionary<string, object> env)
        {
            ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = ResponseBody(env);
            for (var loop = 0; loop != (1 << 20) / (2 << 10); ++loop)
            {
                responseBody.Write(_2KAlphabet, 0, 2048);
            }
        }

        [CanonicalRequest(Path = "/large-immediate-asyncwrite", Description = "Return 1mb ascii byte[] in 2kb await WriteAsync")]
        public async Task LargeImmediateAsyncWrite(IDictionary<string, object> env)
        {
            ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = ResponseBody(env);
            for (var loop = 0; loop != (1 << 20) / (2 << 10); ++loop)
            {
                await responseBody.WriteAsync(_2KAlphabet, 0, 2048);
            }
        }

        [CanonicalRequest(Path = "/large-blockingwork-syncwrite", Description = "Return 1mb ascii byte[] in 2kb sync Write with 20ms thread sleeps every 8 writes")]
        public async Task LargeBlockingWorkSyncWrite(IDictionary<string, object> env)
        {
            ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = ResponseBody(env);
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
            ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            var responseBody = ResponseBody(env);
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
            ResponseHeaders(env)["Content-Type"] = new[] { "text/plain" };
            ResponseBody(env).Write(_2KAlphabet, 0, 2048);
        }
    }
}
