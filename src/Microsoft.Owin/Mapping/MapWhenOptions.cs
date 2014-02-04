// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Options for the MapWhen middleware
    /// </summary>
    public partial class MapWhenOptions
    {
        /// <summary>
        /// The user callback that determines if the branch should be taken
        /// </summary>
        public Func<IOwinContext, bool> Predicate { get; set; }

        /// <summary>
        /// The async user callback that determines if the branch should be taken
        /// </summary>
        public Func<IOwinContext, Task<bool>> PredicateAsync { get; set; }

        /// <summary>
        /// The branch taken for a positive match
        /// </summary>
        public AppFunc Branch { get; set; }
    }
}
