// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.Filters
{
    /// <summary>
    /// Used to apply request filtering for the static file middlewares.
    /// </summary>
    public interface IRequestFilter
    {
        /// <summary>
        /// Indicates if the given request should have access to the given path.
        /// </summary>
        /// <param name="context"></param>
        void ApplyFilter(RequestFilterContext context);
    }
}
