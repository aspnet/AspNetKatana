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
    public static class DefaultFilesExtensions
    {
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string path, string directory)
        {
            return builder.UseDefaultFiles(new StaticFileOptions().WithRequestPath(path).WithPhysicalPath(directory));
        }

        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string path, string directory, IEnumerable<string> defaultFiles)
        {
            return builder.UseDefaultFiles(new StaticFileOptions().WithRequestPath(path).WithPhysicalPath(directory).WithDefaultFiles(defaultFiles));
        }

        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, StaticFileOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(typeof(DefaultFilesMiddleware), options);
        }
    }
}
