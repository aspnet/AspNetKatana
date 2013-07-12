// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    /// <summary>
    /// Contains the options used by the CorsMiddleware
    /// </summary>
    public class CorsOptions
    {
        /// <summary>
        /// The cors policy to apply
        /// </summary>
        public CorsPolicy CorsPolicy { get; set; }

        /// <summary>
        /// The cors engine
        /// </summary>
        public ICorsEngine CorsEngine { get; set; }
    }
}
