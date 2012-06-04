using System;
using System.Text;
using System.Threading;
using Owin;

namespace Gate.Middleware.StaticFiles
{
    internal class TextBody
    {
        private readonly string text;
        private readonly Encoding encoding;
        private BodyStream bodyStream;

        public TextBody(string text, Encoding encoding)
        {
            this.text = text;
            this.encoding = encoding;
        }

        public static BodyDelegate Create(string text, Encoding encoding)
        {
            return (write, flush, end, cancellationToken) =>
            {
                var textBody = new TextBody(text, encoding);
                textBody.Start(write, flush, end, cancellationToken);
            };
        }


        public void Start(Func<ArraySegment<byte>, bool> write, Func<Action, bool> flush, Action<Exception> end, CancellationToken cancellationToken)
        {
            bodyStream = new BodyStream(write, flush, end, cancellationToken);

            Action start = () =>
            {
                try
                {
                    if (bodyStream.CanSend())
                    {
                        var bytes = encoding.GetBytes(text);
                        var segment = new ArraySegment<byte>(bytes);

                        // Not buffered.
                        bodyStream.SendBytes(segment, null, null);

                        bodyStream.Finish();
                    }
                }
                catch (Exception ex)
                {
                    bodyStream.End(ex);
                }
            };

            bodyStream.Start(start, null);
        }
    }
}