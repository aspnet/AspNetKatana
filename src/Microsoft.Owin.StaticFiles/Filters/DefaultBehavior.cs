// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.StaticFiles.Filters
{
    internal static class DefaultBehavior
    {
        private static readonly string[] RestrictedSegments = new[]
        {
            "/bin/",
            "/App_code/",
            "/App_GlobalResources/",
            "/App_LocalResources/",
            "/App_WebReferences/",
            "/App_Data/",
            "/App_Browsers/",
        };

        // Hides specific path segments also blocked by Asp.Net.
        internal static readonly Action<RequestFilterContext> ApplyFilter = context =>
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.Allow();
            string path = context.OwinContext.Request.Path.Value;
            for (int i = 0; i < RestrictedSegments.Length; i++)
            {
                if (path.IndexOf(RestrictedSegments[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    context.PassThrough();
                    break;
                }
            }
        };
    }
}
