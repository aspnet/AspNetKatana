// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    /// <summary>
    /// This middleware provides an efficient fallback mechanism for sending static files
    /// when the server does not natively support such a feature.
    /// The caller is responsible for setting all headers in advance.
    /// The caller is responsible for performing the correct impersonation to give access to the file.
    /// </summary>
    public class SendFileMiddleware : OwinMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public SendFileMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Check if there is a SendFile delegate already presents
            if (context.Get<object>(Constants.SendFileAsyncKey) as SendFileFunc == null)
            {
                context.Set<SendFileFunc>(Constants.SendFileAsyncKey, new SendFileFunc(new SendFileWrapper(context.Response.Body).SendAsync));
            }

            return Next.Invoke(context);
        }

        private class SendFileWrapper
        {
            private readonly Stream _output;

            internal SendFileWrapper(Stream output)
            {
                _output = output;
            }

            // Not safe for overlapped writes.
            internal Task SendAsync(string fileName, long offset, long? length, CancellationToken cancel)
            {
                cancel.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentNullException("fileName");
                }
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException(string.Empty, fileName);
                }

                var fileInfo = new FileInfo(fileName);
                if (offset < 0 || offset > fileInfo.Length)
                {
                    throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
                }

                if (length.HasValue &&
                    (length.Value < 0 || length.Value > fileInfo.Length - offset))
                {
                    throw new ArgumentOutOfRangeException("length", length, string.Empty);
                }

                Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 64,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                try
                {
                    fileStream.Seek(offset, SeekOrigin.Begin);
                    var copyOperation = new StreamCopyOperation(fileStream, _output, length, cancel);
                    return copyOperation.Start()
                        .ContinueWith(resultTask =>
                        {
                            fileStream.Close();
                            resultTask.Wait(); // Throw exceptions, etc.
                        }, TaskContinuationOptions.ExecuteSynchronously);
                }
                catch (Exception)
                {
                    fileStream.Close();
                    throw;
                }
            }
        }
    }
}
