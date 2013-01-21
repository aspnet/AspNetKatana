// -----------------------------------------------------------------------
// <copyright file="StaticFileExtentions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using Owin;

namespace Microsoft.Owin.StaticFiles
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
        /// <param name="directory">The physical directory</param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string directory)
        {
            return UseStaticFiles(builder, new StaticFileOptions().WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enables static file serving for the given request path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The request path</param>
        /// <param name="directory">The physical directory</param>
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
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
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
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, StaticFileOptions options)
        {
            return builder
                .UseSendFileFallback()
                .Use(typeof(StaticFileMiddleware), options);
        }
    }
}
