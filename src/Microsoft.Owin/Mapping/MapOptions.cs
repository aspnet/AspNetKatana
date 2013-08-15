// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Mapping
{
    /// <summary>
    /// Options for the Map middleware
    /// </summary>
    public class MapOptions
    {
        /// <summary>
        /// The path to match
        /// </summary>
        public string PathMatch { get; set; }

        /// <summary>
        /// The branch taken for a positive match
        /// </summary>
        public OwinMiddleware Branch { get; set; }
    }
}
