// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.StaticFiles;

namespace Owin
{
    /// <summary>
    /// Extension methods for the DirectoryBrowserMiddleware
    /// </summary>
    public static class DirectoryBrowserExtensions
    {
        /// <summary>
        /// Enable directory browsing on the current path for the current directory
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseDirectoryBrowser(this IAppBuilder builder)
        {
            return builder.UseDirectoryBrowser(new DirectoryBrowserOptions());
        }

        /// <summary>
        /// Enable directory browsing on the current path for the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directory">The physical directory. This can be relative to the current directory, or an absolute path.</param>
        /// <returns></returns>
        public static IAppBuilder UseDirectoryBrowser(this IAppBuilder builder, string directory)
        {
            return builder.UseDirectoryBrowser(new DirectoryBrowserOptions().WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enable directory browsing on the given path for the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The relative request path.</param>
        /// <param name="directory">The physical directory. This can be relative to the current directory, or an absolute path.</param>
        /// <returns></returns>
        public static IAppBuilder UseDirectoryBrowser(this IAppBuilder builder, string path, string directory)
        {
            return builder.UseDirectoryBrowser(new DirectoryBrowserOptions().WithRequestPath(path).WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enable directory browsing with the given options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IAppBuilder UseDirectoryBrowser(this IAppBuilder builder, DirectoryBrowserOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use<DirectoryBrowserMiddleware>(options);
        }
    }
}
