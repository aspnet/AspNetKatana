// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    /// <summary>
    /// Options for the MapWhen middleware
    /// </summary>
    public class MapWhenOptions
    {
        /// <summary>
        /// The user callback that determines if the branch should be taken
        /// </summary>
        public Func<IOwinContext, bool> Predicate { get; set; }

#if !NET40
        /// <summary>
        /// The async user callback that determines if the branch should be taken
        /// </summary>
        public Func<IOwinContext, Task<bool>> PredicateAsync { get; set; }
#endif

        /// <summary>
        /// The branch taken for a positive match
        /// </summary>
        public OwinMiddleware Branch { get; set; }
    }
}
