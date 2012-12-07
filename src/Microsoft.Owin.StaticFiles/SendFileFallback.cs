// -----------------------------------------------------------------------
// <copyright file="SendFileFallback.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    // This middleware provides an efficient fallback mechanism for sending static files
    // when the server does not natively support such a feature.
    // The caller is responsible for setting all headers in advance.
    // The caller is responsible for performing the correct impersonation to give access to the file.
    // TODO: Pool buffers between operations.
    public class SendFileFallback
    {
        private AppFunc _next;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public SendFileFallback(AppFunc next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if there is a SendFile delegate already present
            object obj;
            if (!environment.TryGetValue(Constants.SendFileAsyncKey, out obj) || !(obj is SendFileFunc))
            {
                Stream output = (Stream)environment[Constants.ResponseBodyKey];
                environment[Constants.SendFileAsyncKey] = new SendFileFunc(new SendFileWrapper(output).SendAsync);
            }

            return _next(environment);
        }

        private class SendFileWrapper
        {
            private Stream _output;

            internal SendFileWrapper(Stream output)
            {
                _output = output;
            }

            // TODO: Detect and throw if the caller tries to start a second operation before the first finishes.
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

                FileInfo fileInfo = new FileInfo(fileName);
                if (offset < 0 || offset > fileInfo.Length)
                {
                    throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
                }

                if (length.HasValue &&
                    (length.Value < 0 || length.Value > fileInfo.Length - offset))
                {
                    throw new ArgumentOutOfRangeException("length", length, string.Empty);
                }

                Stream fileStream = File.OpenRead(fileName);
                try
                {
                    fileStream.Seek(offset, SeekOrigin.Begin);
                    StreamCopyOperation copyOperation = new StreamCopyOperation(fileStream, _output, length, cancel);
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
