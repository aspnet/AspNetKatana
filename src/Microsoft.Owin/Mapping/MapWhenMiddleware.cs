// <copyright file="MapWhenMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Mapping
{
    /// <summary>
    /// Determines if the request should take a specific branch of the pipeline by passing the environment
    /// to a user defined callback.
    /// </summary>
    public class MapWhenMiddleware : OwinMiddleware
    {
        private readonly MapWhenOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next">The normal application pipeline</param>
        /// <param name="branch">The branch to take on a true result</param>
        /// <param name="predicate">The user callback that determines if the branch should be taken</param>
        public MapWhenMiddleware(OwinMiddleware next, MapWhenOptions options)
            : base(next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            _options = options;
        }

#if NET40
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

            if (_options.Predicate(context))
            {
                return _options.Branch.Invoke(context);
            }
            else
            {
                return Next.Invoke(context);
            }
        }
#else
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (_options.Predicate != null)
            {
                if (_options.Predicate(context))
                {
                    await _options.Branch.Invoke(context);
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
            else 
            {
                if (await _options.PredicateAsync(context))
                {
                    await _options.Branch.Invoke(context);
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
        }
#endif
    }
}
