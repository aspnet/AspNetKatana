using System;
using System.IO;
using System.Threading;
using Owin;

namespace Gate.Middleware.StaticFiles
{
    internal class FileBody
    {
        private FileStream fileStream;
        private readonly Tuple<long, long> range;
        private readonly string path;
        private BodyStream bodyStream;

        public FileBody(string path, Tuple<long, long> range)
        {
            this.path = path;
            this.range = range;
        }

        public static BodyDelegate Create(string path, Tuple<long, long> range)
        {
            return (write, flush, end, cancel) =>
            {
                var fileBody = new FileBody(path, range);
                fileBody.Start(write, flush, end, cancel);
            };
        }

        void Start(Func<ArraySegment<byte>, bool> write, Func<Action, bool> flush, Action<Exception> end, CancellationToken cancellationToken)
        {
            bodyStream = new BodyStream(write, flush, end, cancellationToken);

            Action start = () =>
            {
                try
                {
                    var rangeLength = range.Item2 - range.Item1 + 1;
                    SendFile(rangeLength);
                }
                catch (Exception ex)
                {
                    bodyStream.End(ex);
                }
            };

            Action dispose = () =>
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                    fileStream = null;
                }
            };

            bodyStream.Start(start, dispose);
        }

        private void SendFile(long length)
        {
            if (bodyStream.CanSend())
            {
                EnsureOpenFileStream();
                SendBuffer(length);
            }
        }

        private void EnsureOpenFileStream()
        {
            if (fileStream == null || !fileStream.CanRead)
            {
                fileStream = File.OpenRead(path);
                fileStream.Seek(range.Item1, SeekOrigin.Begin);
            }
        }

        private void SendBuffer(long length)
        {
            var segmentInfo = ReadNextBuffer(length);
            var buffer = segmentInfo.Item1;
            var bytesRead = segmentInfo.Item2;

            if (bytesRead == 0)
            {
                bodyStream.Finish();
                return;
            }

            length -= bytesRead;

            Action nextCall = () => SendFile(length);

            bodyStream.SendBytes(buffer, nextCall, nextCall);
        }

        private Tuple<ArraySegment<byte>, long> ReadNextBuffer(long remainingLength)
        {
            const long maxBufferSize = 64 * 1024;
            var bufferSize = remainingLength < maxBufferSize ? remainingLength : maxBufferSize;
            var part = new byte[bufferSize];
            var bytesRead = fileStream.Read(part, 0, (int)bufferSize);

            return new Tuple<ArraySegment<byte>, long>(new ArraySegment<byte>(part), bytesRead);
        }
    }
}