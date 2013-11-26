// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin;
using Microsoft.Owin.StaticFiles;

namespace Owin
{
    /// <summary>
    /// Extension methods for the DefaultFilesMiddleware
    /// </summary>
    public static class DefaultFilesExtensions
    {
        /// <summary>
        /// Enables default file serving on the current path from the current directory
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions());
        }

        /// <summary>
        /// Enables default file serving on the given path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="requestPath">The relative request path.</param>
        /// <param name="physicalPath">The physical directory. This can be relative to the current directory, or an absolute path.</param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string requestPath, string physicalPath)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions() { RequestPath = new PathString(requestPath) }.WithPhysicalPath(physicalPath));
        }

        /// <summary>
        /// Enables default file serving with the given options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, DefaultFilesOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use<DefaultFilesMiddleware>(options);
        }
    }
}
