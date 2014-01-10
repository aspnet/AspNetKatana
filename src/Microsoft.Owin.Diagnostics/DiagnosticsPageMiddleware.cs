// <copyright file="TestPage.cs" company="Microsoft Open Technologies, Inc.">
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

#if DEBUG
using System;
using System.Threading.Tasks;
using Microsoft.Owin.Diagnostics.Views;

namespace Microsoft.Owin.Diagnostics
{
    /// <summary>
    /// A human readable page with basic debugging actions.
    /// </summary>
    public class DiagnosticsPageMiddleware : OwinMiddleware
    {
        private readonly DiagnosticsPageOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsPageMiddleware"/> class
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public DiagnosticsPageMiddleware(OwinMiddleware next, DiagnosticsPageOptions options)
            : base(next)
        {
            _options = options;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!_options.Path.HasValue || _options.Path == context.Request.Path)
            {
                var page = new DiagnosticsPage();
                page.Execute(context);
                return CompletedTask();
            }
            return Next.Invoke(context);
        }

        private static Task CompletedTask()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}
#endif