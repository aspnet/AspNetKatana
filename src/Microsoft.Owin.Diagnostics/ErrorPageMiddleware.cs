// <copyright file="ShowExceptionsMiddleware.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.Diagnostics
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
    /// </summary>
    public class ErrorPageMiddleware
    {
        private readonly AppFunc _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public ErrorPageMiddleware(AppFunc next)
        {
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "For diagnostics")]
        public Task Invoke(IDictionary<string, object> environment)
        {
            try
            {
                return _next(environment).ContinueWith(appTask =>
                    {
                        if (appTask.IsFaulted)
                        {
                            return DisplayException(environment, appTask.Exception);
                        }
                        if (appTask.IsCanceled)
                        {
                            return DisplayException(environment, new TaskCanceledException(appTask));
                        }
                        return CompletedTask();
                    });
            }
            catch (Exception ex)
            {
                return DisplayException(environment, ex);
            }
        }

        private static Task CompletedTask()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }

        // TODO: Eventually make this nicely layed out.
        // Assumes the response headers have not been sent.  If they have, still attempt to write to the body.
        private static Task DisplayException(IDictionary<string, object> environment, Exception ex)
        {
            environment["owin.ResponseStatusCode"] = 500;
            environment["owin.ResponseReasonPhrase"] = "Internal Server Error";

            byte[] data = Encoding.UTF8.GetBytes(ex.ToString());

            Stream responseStream = (Stream)environment["owin.ResponseBody"];
            IDictionary<string, string[]> responseHeaders =
                (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            responseHeaders["Content-Length"] = new string[] { data.Length.ToString(CultureInfo.InvariantCulture) };
            responseHeaders["Content-Type"] = new string[] { "text/plain" };

            return Task.Factory.FromAsync(responseStream.BeginWrite, responseStream.EndWrite, data, 0, data.Length, null);
            // 4.5: return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}
