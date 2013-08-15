// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    /// <summary>
    /// Options for the MapWhen middleware
    /// </summary>
    public partial class MapWhenOptions
    {
        /// <summary>
        /// The async user callback that determines if the branch should be taken
        /// </summary>
        public Func<IOwinContext, Task<bool>> PredicateAsync { get; set; }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
