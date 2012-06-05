using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Owin;

namespace Gate.Middleware
{
    using Response = Tuple<string, IDictionary<string, string[]>, BodyDelegate>;

    internal static class ContentLength
    {
        public static IAppBuilder UseContentLength(this IAppBuilder builder)
        {
            return builder.Use<AppDelegate>(Middleware);
        }


        public static AppDelegate Middleware(AppDelegate app)
        {
            return
                (env, result, fault) =>
                    app(
                        env,
                        (status, headers, body) =>
                        {
                            if (IsStatusWithNoNoEntityBody(status) ||
                                headers.ContainsKey("Content-Length") ||
                                headers.ContainsKey("Transfer-Encoding"))
                            {
                                result(status, headers, body);
                            }
                            else
                            {
                                var token = CancellationToken.None;
                                object obj;
                                if (env.TryGetValue(typeof(CancellationToken).FullName, out obj) && obj is CancellationToken)
                                    token = (CancellationToken)obj;

                                var buffer = new DataBuffer();
                                body(
                                    buffer.Add,
                                    ex =>
                                    {
                                        buffer.End(ex);
                                        headers["Content-Length"] = new[] { buffer.GetCount().ToString() };
                                        result(status, headers, buffer.Body);
                                    },
                                    token);
                            }
                        },
                        fault);
        }



        private static bool IsStatusWithNoNoEntityBody(string status)
        {
            return status.StartsWith("1") ||
                status.StartsWith("204") ||
                status.StartsWith("205") ||
                status.StartsWith("304");
        }

        class DataBuffer
        {
            readonly List<ArraySegment<byte>> _buffers = new List<ArraySegment<byte>>();
            ArraySegment<byte> _tail = new ArraySegment<byte>(new byte[2048], 0, 0);
            Exception _error;

            public int GetCount()
            {
                return _buffers.Aggregate(0, (c, d) => c + d.Count);
            }

            public bool Add(ArraySegment<byte> data, Action continuation)
            {
                var remaining = data;
                while (remaining.Count != 0)
                {
                    if (_tail.Count + _tail.Offset == _tail.Array.Length)
                    {
                        _buffers.Add(_tail);
                        _tail = new ArraySegment<byte>(new byte[4096], 0, 0);
                    }
                    var copyCount = Math.Min(remaining.Count, _tail.Array.Length - _tail.Offset - _tail.Count);
                    Array.Copy(remaining.Array, remaining.Offset, _tail.Array, _tail.Offset + _tail.Count, copyCount);
                    _tail = new ArraySegment<byte>(_tail.Array, _tail.Offset, _tail.Count + copyCount);
                    remaining = new ArraySegment<byte>(remaining.Array, remaining.Offset + copyCount, remaining.Count - copyCount);
                }
                return false;
            }

            public void End(Exception error)
            {
                _buffers.Add(_tail);
                _error = error;
            }

            public void Body(
                Func<ArraySegment<byte>, Action, bool> write,
                Action<Exception> end,
                CancellationToken cancel)
            {
                try
                {
                    foreach (var data in _buffers)
                    {
                        if (cancel.IsCancellationRequested)
                            break;

                        write(data, null);
                    }
                    end(_error);
                }
                catch (Exception ex)
                {
                    end(ex);
                }
            }
        }

    }
}

