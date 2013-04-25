// <copyright file="SecurityHelperLookupResult.cs" company="Microsoft Open Technologies, Inc.">
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

#if NET45

using System.Security.Claims;

namespace Microsoft.Owin.Security.Infrastructure
{
    /// <summary>
    /// Information returned by various SecurityHelper lookup methods
    /// </summary>
    public struct SecurityHelperLookupResult
    {
        private readonly bool _shouldHappen;
        private readonly ClaimsIdentity _identity;

        /// <summary>
        /// Initialize the properties
        /// </summary>
        /// <param name="shouldHappen">Assigned to the ShouldHappen property</param>
        public SecurityHelperLookupResult(bool shouldHappen)
        {
            _shouldHappen = shouldHappen;
            _identity = null;
        }

        /// <summary>
        /// Initialize the properties
        /// </summary>
        /// <param name="shouldHappen">Assigned to the ShouldHappen property</param>
        /// <param name="identity">Assigned to the Identity property</param>
        public SecurityHelperLookupResult(bool shouldHappen, ClaimsIdentity identity)
        {
            _shouldHappen = shouldHappen;
            _identity = identity;
        }

        /// <summary>
        /// True if the middleware should act on the request or response
        /// </summary>
        public bool ShouldHappen
        {
            get { return _shouldHappen; }
        }

        /// <summary>
        /// Informs the middleware of the claims that add detail to the challenge or signin that should be performed
        /// </summary>
        public ClaimsIdentity Identity
        {
            get { return _identity; }
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
