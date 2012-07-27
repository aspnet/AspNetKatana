using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gate.Middleware.Utils;
using Owin;
using System.Text;
using Gate.Utils;

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
            return call => app(call).Then(result =>
            {
                if (!IsStatusWithNoNoEntityBody(result.Status) &&
                    !result.Headers.ContainsKey("Content-Length") &&
                    !result.Headers.ContainsKey("Transfer-Encoding"))
                {
                    result.Headers.AddHeader("Transfer-Encoding", "chunked");
                    result.Body = WrapOutputStream(result.Body);
                }
                return result;
            });
        }

        public static BodyDelegate WrapOutputStream(BodyDelegate body)
        {
            return (output, cancel) =>
                body(new StreamWrapper(output, OnWriteFilter), cancel)
                    .Then(() =>
                        output.WriteAsync(FinalChunk.Array, FinalChunk.Offset, FinalChunk.Count));
        }

        public static ArraySegment<byte>[] OnWriteFilter(ArraySegment<byte> data)
        {
            return data.Count == 0
                ? new[]
                {
                    data
                }
                : new[]
                {
                    ChunkPrefix((uint) data.Count), 
                    data, 
                    EndOfChunk,
                };
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

        private static bool IsStatusWithNoNoEntityBody(int status)
        {
            return (status >= 100 && status < 200) ||
                status == 204 ||
                status == 205 ||
                status == 304;
        }
    }
}

