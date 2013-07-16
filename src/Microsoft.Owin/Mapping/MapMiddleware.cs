// <copyright file="MapMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    /// <summary>
    /// Used to create path based branches in your application pipeline.
    /// The owin.RequestPathBase is not included in the evaluation, only owin.RequestPath.
    /// Matching paths have the matching piece removed from owin.RequestPath and added to the owin.RequestPathBase.
    /// </summary>
    public class MapMiddleware : OwinMiddleware
    {
        private readonly OwinMiddleware _branch;
        private readonly string _pathMatch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next">The normal pipeline taken for a negative match</param>
        /// <param name="branch">The branch taken for a positive match</param>
        /// <param name="pathMatch">The path to match</param>
        public MapMiddleware(OwinMiddleware next, string pathMatch, OwinMiddleware branch) : base(next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (pathMatch == null)
            {
                throw new ArgumentNullException("pathMatch");
            }
            if (branch == null)
            {
                throw new ArgumentNullException("branch");
            }
            // Must at least start with a "/foo" to be considered a branch. Otherwise it's a catch-all.
            if (!pathMatch.StartsWith("/", StringComparison.Ordinal) || pathMatch.Length == 1)
            {
                throw new ArgumentException(Resources.Exception_PathMustStartWithSlash, "pathMatch");
            }

            // Only match on "/" boundaries, but permit the trailing "/" to be absent.
            if (pathMatch.EndsWith("/", StringComparison.Ordinal))
            {
                pathMatch = pathMatch.Substring(0, pathMatch.Length - 1);
            }

            _pathMatch = pathMatch;
            _branch = branch;
        }

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

            var path = context.Request.Path;

            // Only match on "/" boundaries.
            if (path.StartsWith(_pathMatch, StringComparison.OrdinalIgnoreCase)
                && (path.Length == _pathMatch.Length
                    || path[_pathMatch.Length] == '/'))
            {
                // Update the path
                var pathBase = context.Request.PathBase;
                string subpath = path.Substring(_pathMatch.Length);
                context.Request.PathBase = pathBase + _pathMatch;
                context.Request.Path = subpath;

                await _branch.Invoke(context);

                context.Request.PathBase = pathBase;
                context.Request.Path = path;
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }
}
#endif
