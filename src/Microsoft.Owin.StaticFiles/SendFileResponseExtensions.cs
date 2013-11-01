// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.StaticFiles;

namespace Microsoft.Owin
{
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    /// <summary>
    /// Provides extensions for IOwinResponse exposing the SendFile extension.
    /// </summary>
    public static class SendFileResponseExtensions
    {
        /// <summary>
        /// Checks if the SendFile extension is supported.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>True if sendfile.SendAsync is defined in the environment.</returns>
        public static bool SupportsSendFile(this IOwinResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            return response.Get<SendFileFunc>(Constants.SendFileAsyncKey) != null;
        }

        /// <summary>
        /// Sends the given file using the SendFile extension.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Task SendFileAsync(this IOwinResponse response, string fileName)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            return response.SendFileAsync(fileName, 0, null, CancellationToken.None);
        }

        /// <summary>
        /// Sends the given file using the SendFile extension.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileName"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task SendFileAsync(this IOwinResponse response, string fileName, long offset, long? count, CancellationToken cancellationToken)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            SendFileFunc sendFileFunc = response.Get<SendFileFunc>(Constants.SendFileAsyncKey);
            if (sendFileFunc == null)
            {
                throw new NotSupportedException(Resources.Exception_SendFileNotSupported);
            }

            return sendFileFunc(fileName, offset, count, cancellationToken);
        }
    }
}
