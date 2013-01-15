// -----------------------------------------------------------------------
// <copyright file="FileServerExtensions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using Owin;

namespace Microsoft.Owin.StaticFiles
{
    public static class FileServerExtensions
    {
        public static IAppBuilder UseFileServer(this IAppBuilder builder, string path, string directory)
        {
            return UseFileServer(builder, options => options.WithRequestPath(path).WithPhysicalPath(directory));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFileServer(this IAppBuilder builder, Action<FileServerOptions> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var options = new FileServerOptions();
            configuration(options);
            return UseFileServer(builder, options);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFileServer(this IAppBuilder builder, FileServerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            return builder
                .UseDefaultFiles(options.DefaultFilesOptions)
                .UseDirectoryBrowser(options.DirectoryBrowserOptions)
                .UseSendFileFallback()
                .UseStaticFiles(options.StaticFileOptions);
        }
    }
}
