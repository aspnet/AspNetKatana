// <copyright file="MapPathMiddleware.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Used to create path based branches in your application pipeline.
    /// The owin.RequestPathBase is not included in the evaluation, only owin.RequestPath.
    /// Matching paths have the matching piece removed from owin.RequestPath and added to the owin.RequestPathBase.
    /// </summary>
    public class MapPathMiddleware
    {
        private readonly AppFunc _next;
        private readonly AppFunc _branch;
        private readonly string _pathMatch;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next">The normal pipeline taken for a negative match</param>
        /// <param name="branch">The branch taken for a positive match</param>
        /// <param name="pathMatch">The path to match</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public MapPathMiddleware(AppFunc next, AppFunc branch, string pathMatch)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (branch == null)
            {
                throw new ArgumentNullException("branch");
            }
            if (pathMatch == null)
            {
                throw new ArgumentNullException("pathMatch");
            }
            // Must at least start with a "/foo" to be considered a branch. Otherwise it's a catch-all.
            if (!pathMatch.StartsWith("/", StringComparison.Ordinal) || pathMatch.Length == 1)
            {
                throw new ArgumentException("Path must start with a \"/\" followed by one or more characters.", "pathMatch");
            }

            // Only match on "/" boundaries, but permit the trailing "/" to be absent.
            if (pathMatch.EndsWith("/", StringComparison.Ordinal))
            {
                pathMatch = pathMatch.Substring(0, pathMatch.Length - 1);
            }

            _next = next;
            _branch = branch;
            _pathMatch = pathMatch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            string path = (string)environment["owin.RequestPath"];

            // Only match on "/" boundaries.
            if (path.StartsWith(_pathMatch, StringComparison.OrdinalIgnoreCase)
                && (path.Length == _pathMatch.Length
                    || path[_pathMatch.Length] == '/'))
            {
                // Update the path
                string pathBase = (string)environment["owin.RequestPathBase"];
                string subpath = path.Substring(_pathMatch.Length);
                environment["owin.RequestPathBase"] = pathBase + _pathMatch;
                environment["owin.RequestPath"] = subpath;

                return _branch(environment);
            }

            return _next(environment);
        }
    }
}
