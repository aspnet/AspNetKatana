// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.StaticFiles;

namespace Owin
{
    /// <summary>
    /// Extension methods for the StaticFileMiddleware
    /// </summary>
    public static class StaticFileExtensions
    {
        /// <summary>
        /// Enables static file serving for the current request path from the current directory
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder)
        {
            return UseStaticFiles(builder, new StaticFileOptions());
        }

        /// <summary>
        /// Enables static file serving for the current request path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directory">The physical directory. This can be relative to the current directory, or an absolute path.</param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string directory)
        {
            return UseStaticFiles(builder, new StaticFileOptions().WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enables static file serving for the given request path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The relative request path.</param>
        /// <param name="directory">The physical directory. This can be relative to the current directory, or an absolute path.</param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string path, string directory)
        {
            return UseStaticFiles(builder, new StaticFileOptions().WithRequestPath(path).WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enables static file serving with the given options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The configuration callback</param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, Action<StaticFileOptions> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var options = new StaticFileOptions();
            configuration(options);
            return UseStaticFiles(builder, options);
        }

        /// <summary>
        /// Enables static file serving with the given options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, StaticFileOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            return builder.Use<StaticFileMiddleware>(options);
        }
    }
}
