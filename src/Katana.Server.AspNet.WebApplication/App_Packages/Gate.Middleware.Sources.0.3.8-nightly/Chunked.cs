using System;
using System.Collections.Generic;
using Owin;
using System.Text;

namespace Gate.Middleware
{
    internal static class Chunked
    {
        public static IAppBuilder UseChunked(this IAppBuilder builder)
        {
            return builder.Use<AppDelegate>(Middleware);
        }

        static readonly ArraySegment<byte> EndOfChunk = new ArraySegment<byte>(Encoding.ASCII.GetBytes("\r\n"));
        static readonly ArraySegment<byte> FinalChunk = new ArraySegment<byte>(Encoding.ASCII.GetBytes("0\r\n\r\n"));
        static readonly byte[] Hex = Encoding.ASCII.GetBytes("0123456789abcdef\r\n");

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
                                headers["Transfer-Encoding"] = new[] { "chunked" };

                                result(
                                    status,
                                    headers,
                                    (write, flush, end, cancel) =>
                                        body(
                                            data =>
                                            {
                                                if (data.Count == 0)
                                                {
                                                    return write(data);
                                                }

                                                write(ChunkPrefix((uint) data.Count));
                                                write(data);
                                                return write(EndOfChunk);
                                            },
                                            flush,
                                            ex =>
                                            {
                                                write(FinalChunk);
                                                end(ex);
                                            },
                                            cancel));
                            }
                        },
                        fault);
        }

        public static ArraySegment<byte> ChunkPrefix(uint dataCount)
        {
            var prefixBytes = new[]
            {
                Hex[(dataCount >> 28) & 0xf],
                Hex[(dataCount >> 24) & 0xf],
                Hex[(dataCount >> 20) & 0xf],
                Hex[(dataCount >> 16) & 0xf],
                Hex[(dataCount >> 12) & 0xf],
                Hex[(dataCount >> 8) & 0xf],
                Hex[(dataCount >> 4) & 0xf],
                Hex[(dataCount >> 0) & 0xf],
                Hex[16],
                Hex[17],
            };
            var shift = (dataCount & 0xffff0000) == 0 ? 16 : 0;
            shift += ((dataCount << shift) & 0xff000000) == 0 ? 8 : 0;
            shift += ((dataCount << shift) & 0xf0000000) == 0 ? 4 : 0;
            return new ArraySegment<byte>(prefixBytes, shift / 4, 10 - shift / 4);
        }

        private static bool IsStatusWithNoNoEntityBody(string status)
        {
            return status.StartsWith("1") ||
                status.StartsWith("204") ||
                status.StartsWith("205") ||
                status.StartsWith("304");
        }
    }
}

