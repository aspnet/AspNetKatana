// <copyright file="StaticFileMiddleware.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    /// <summary>
    /// Enables serving static files for a given request path
    /// </summary>
    public class StaticFileMiddleware
    {
        private readonly AppFunc _next;
        private readonly StaticFileOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public StaticFileMiddleware(AppFunc next, StaticFileOptions options)
        {
            _next = next;
            _options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if the URL matches any expected paths
            var context = new StaticFileContext(environment, _options);
            if (context.ValidateMethod()
                && context.ValidatePath()
                && context.LookupContentType()
                && context.LookupFileInfo())
            {
                context.ComprehendRequestHeaders();
                context.ApplyResponseHeaders();

                var preconditionState = context.GetPreconditionState();
                if (preconditionState == StaticFileContext.PreconditionState.NotModified)
                {
                    return context.SendStatusAsync(304);
                }
                if (preconditionState == StaticFileContext.PreconditionState.PreconditionFailed)
                {
                    return context.SendStatusAsync(412);
                }
                if (context.IsHeadMethod)
                {
                    return context.SendStatusAsync(200);
                }
                return context.SendAsync(200);
            }

            return _next(environment);
        }
    }
}
