// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Options for the Map middleware
    /// </summary>
    public class MapOptions
    {
        /// <summary>
        /// The path to match
        /// </summary>
        public PathString PathMatch { get; set; }

        /// <summary>
        /// The branch taken for a positive match
        /// </summary>
        public AppFunc Branch { get; set; }
    }
}
