// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Flags used to select one or more HTTP headers.
    /// </summary>
    [Flags]
    public enum HeaderFields
    {
        /// <summary>
        /// No headers selected
        /// </summary>
        None = 0,

        /// <summary>
        /// Last-Modified
        /// </summary>
        LastModified = 1,

        /// <summary>
        /// E-Tag
        /// </summary>
        ETag = 2,

        /// <summary>
        /// Expires
        /// </summary>
        Expires = 4,

        /// <summary>
        /// Cache-Control
        /// </summary>
        CacheControl = 8,
    }
}
