// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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
        /// Enables default file serving on the current path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directory">The physical file system directory</param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string directory)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions().WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enables default file serving on the given path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The request path</param>
        /// <param name="directory">The physical file system directory</param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string path, string directory)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions().WithRequestPath(path).WithPhysicalPath(directory));
        }

        /// <summary>
        /// Serves the given file names by default for the given path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The request path</param>
        /// <param name="directory">The physical file system directory</param>
        /// <param name="defaultFiles">The default file names</param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string path, string directory, params string[] defaultFiles)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions().WithRequestPath(path).WithPhysicalPath(directory).WithDefaultFileNames(defaultFiles));
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
