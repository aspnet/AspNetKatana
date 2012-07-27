using System;
using System.Text;
using System.Threading;
using Owin;
using System.IO;
using System.Threading.Tasks;

namespace Gate.Middleware.StaticFiles
{
    internal class TextBody
    {
        private readonly string text;
        private readonly Encoding encoding;

        public TextBody(string text, Encoding encoding)
        {
            this.text = text;
            this.encoding = encoding;
        }

        public static BodyDelegate Create(string text, Encoding encoding)
        {
            return (stream, cancel) =>
            {
                var textBody = new TextBody(text, encoding);
                return textBody.Start(stream, cancel);
            };
        }


        public Task Start(Stream stream, CancellationToken cancellationToken)
        {
            TaskCompletionSource<object> completed = new TaskCompletionSource<object>();
            var bytes = encoding.GetBytes(text);
            stream.BeginWrite(bytes, 0, bytes.Length,
                async =>
                {
                    try
                    {
                        stream.EndWrite(async);
                        completed.TrySetResult(null);
                    }
                    catch (Exception ex)
                    {
                        completed.TrySetException(ex);
                    }
                },
                null);

            return completed.Task;
        }
    }
}