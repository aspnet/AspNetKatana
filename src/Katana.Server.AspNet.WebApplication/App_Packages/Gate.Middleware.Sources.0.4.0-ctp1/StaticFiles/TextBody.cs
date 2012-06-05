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
            return (write, end, cancel) =>
            {
                var textBody = new TextBody(text, encoding);
                textBody.Start(write, end, cancel);
            };
        }


        public void Start(Func<ArraySegment<byte>, Action, bool> write, Action<Exception> end, CancellationToken cancellationToken)
        {
            bodyStream = new BodyStream(write, end, cancellationToken);

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