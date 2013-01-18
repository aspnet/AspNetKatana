// -----------------------------------------------------------------------
// <copyright file="DirectoryBrowserExtensions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Owin;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Extension methods for the DefaultFilesMiddleware
    /// </summary>
    public static class DefaultFilesExtensions
    {
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
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string path, string directory, IEnumerable<string> defaultFiles)
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

            return builder.Use(typeof(DefaultFilesMiddleware), options);
        }
    }
}
