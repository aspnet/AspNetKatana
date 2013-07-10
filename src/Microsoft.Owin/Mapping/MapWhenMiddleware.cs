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

#if !NET40
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    using Predicate = Func<IOwinContext, bool>;
    using PredicateAsync = Func<IOwinContext, Task<bool>>;

    /// <summary>
    /// Determines if the request should take a specific branch of the pipeline by passing the environment
    /// to a user defined callback.
    /// </summary>
    public class MapWhenMiddleware : OwinMiddleware
    {
        private readonly OwinMiddleware _branch;
        private readonly Predicate _predicate;
        private readonly PredicateAsync _predicateAsync;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next">The normal application pipeline</param>
        /// <param name="branch">The branch to take on a true result</param>
        /// <param name="predicate">The user callback that determines if the branch should be taken</param>
        public MapWhenMiddleware(OwinMiddleware next, Predicate predicate, OwinMiddleware branch)
            : base(next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (branch == null)
            {
                throw new ArgumentNullException("branch");
            }
 
            _predicate = predicate;
            _branch = branch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next">The normal application pipeline</param>
        /// <param name="branch">The branch to take on a true result</param>
        /// <param name="predicateAsync">The async user callback that determines if the branch should be taken</param>
        public MapWhenMiddleware(OwinMiddleware next, PredicateAsync predicateAsync, OwinMiddleware branch)
            : base(next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (predicateAsync == null)
            {
                throw new ArgumentNullException("predicateAsync");
            }
            if (branch == null)
            {
                throw new ArgumentNullException("branch");
            }

            _predicateAsync = predicateAsync;
            _branch = branch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public override async Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (_predicate != null)
            {
                if (_predicate(context))
                {
                    await _branch.Invoke(context);
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
            else
            {
                if (await _predicateAsync(context))
                {
                    await _branch.Invoke(context);
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
        }
    }
}
#endif
