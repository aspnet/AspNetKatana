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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Owin.Diagnostics.Views;
using Owin.Types;

namespace Microsoft.Owin.Diagnostics
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// A human readable page with basic debugging actions.
    /// </summary>
    public class DiagnosticsPageMiddleware
    {
        private readonly AppFunc _next;
        private readonly DiagnosticsPageOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public DiagnosticsPageMiddleware(AppFunc next, DiagnosticsPageOptions options)
        {
            _next = next;
            _options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Owin.Types.OwinResponse.WriteAsync(System.String)", Justification = "Generating non-localized content.")]
        public Task Invoke(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);
            if (string.IsNullOrEmpty(_options.Path) || string.Equals(request.Path, _options.Path, StringComparison.OrdinalIgnoreCase))
            {
                var page = new DiagnosticsPage();
                page.Execute(environment);
                return CompletedTask();
            }
            return _next(environment);
        }

        private static Task CompletedTask()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}
