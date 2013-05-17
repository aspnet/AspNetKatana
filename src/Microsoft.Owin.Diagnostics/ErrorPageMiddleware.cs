// <copyright file="ErrorPageMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Diagnostics.Views;

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
                        return DisplayExceptionWrapper(environment, appTask.Exception);
                    }
                    if (appTask.IsCanceled)
                    {
                        return DisplayExceptionWrapper(environment, new TaskCanceledException(appTask));
                    }
                    return CompletedTask();
                });
            }
            catch (Exception ex)
            {
                // If there's a Exception while generating the error page, re-throw the original exception.
                try
                {
                    return DisplayException(environment, ex);
                }
                catch (Exception)
                {
                }

                throw;
            }
        }

        private static Task CompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }

        private static Task FaultedTask(Exception ex)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        // If there's a Exception while generating the error page, re-throw the original exception.
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to re-throw the original.")]
        private static Task DisplayExceptionWrapper(IDictionary<string, object> environment, Exception ex)
        {
            try
            {
                return DisplayException(environment, ex);
            }
            catch (Exception)
            {
                return FaultedTask(ex);
            }
        }

        // Assumes the response headers have not been sent.  If they have, still attempt to write to the body.
        private static Task DisplayException(IDictionary<string, object> environment, Exception ex)
        {
            var request = new OwinRequest(environment);
            var errorPage = new ErrorPage
            {
                Model = new ErrorPageModel
                {
                    Error = ex,
                    StackFrames = StackFrames(ex),
                    Environment = environment,
                    Query = request.GetQuery(),
                    Cookies = request.GetCookies(),
                    Headers = request.Headers,
                }
            };
            errorPage.Execute(environment);
            return CompletedTask();
        }

        private static IEnumerable<StackFrame> StackFrames(Exception ex)
        {
            return StackFrames(StackTraces(ex).Reverse());
        }

        private static IEnumerable<string> StackTraces(Exception ex)
        {
            for (Exception scan = ex; scan != null; scan = scan.InnerException)
            {
                yield return ex.StackTrace;
            }
        }

        private static IEnumerable<StackFrame> StackFrames(IEnumerable<string> stackTraces)
        {
            foreach (var stackTrace in stackTraces.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                var heap = new Chunk { Text = stackTrace + "\r\n", End = stackTrace.Length + 2 };
                for (Chunk line = heap.Advance("\r\n"); line.HasValue; line = heap.Advance("\r\n"))
                {
                    yield return StackFrame(line);
                }
            }
        }

        private static StackFrame StackFrame(Chunk line)
        {
            line.Advance("  at ");
            string function = line.Advance(" in ").ToString();
            string file = line.Advance(":line ").ToString();
            int lineNumber = line.ToInt32();

            return string.IsNullOrEmpty(file)
                ? LoadFrame(line.ToString(), string.Empty, 0)
                : LoadFrame(function, file, lineNumber);
        }

        private static StackFrame LoadFrame(string function, string file, int lineNumber)
        {
            var frame = new StackFrame { Function = function, File = file, Line = lineNumber };
            if (File.Exists(file))
            {
                string[] code = File.ReadAllLines(file);
                frame.PreContextLine = Math.Max(lineNumber - 6, 1);
                frame.PreContextCode = code.Skip(frame.PreContextLine - 1).Take(lineNumber - frame.PreContextLine).ToArray();
                frame.ContextCode = code.Skip(lineNumber - 1).FirstOrDefault();
                frame.PostContextCode = code.Skip(lineNumber).Take(6).ToArray();
            }
            return frame;
        }

        internal class Chunk
        {
            public string Text;
            public int Start;
            public int End;

            public bool HasValue
            {
                get { return Text != null; }
            }

            public Chunk Advance(string delimiter)
            {
                int indexOf = HasValue ? Text.IndexOf(delimiter, Start, End - Start, StringComparison.Ordinal) : -1;
                if (indexOf < 0)
                {
                    return new Chunk();
                }

                var chunk = new Chunk { Text = Text, Start = Start, End = indexOf };
                Start = indexOf + delimiter.Length;
                return chunk;
            }

            public override string ToString()
            {
                return HasValue ? Text.Substring(Start, End - Start) : string.Empty;
            }

            public int ToInt32()
            {
                int value;
                return HasValue && Int32.TryParse(
                    Text.Substring(Start, End - Start),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value) ? value : 0;
            }
        }
    }
}
