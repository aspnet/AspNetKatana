// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.StaticFiles.Filters
{
    /// <summary>
    /// A default request filter that prevents access to some potentially private path segments.
    /// </summary>
    public class RequestFilter : IRequestFilter
    {
        /// <summary>
        /// Creates a new request filter.
        /// </summary>
        public RequestFilter()
        {
            OnApplyFilter = DefaultBehavior.ApplyFilter;
        }

        /// <summary>
        /// Changes the request filter action.
        /// </summary>
        public Action<RequestFilterContext> OnApplyFilter { get; set; }

        /// <summary>
        /// Executes the specified request filter action.
        /// </summary>
        /// <param name="context"></param>
        public virtual void ApplyFilter(RequestFilterContext context)
        {
            OnApplyFilter.Invoke(context);
        }
    }
}
