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
    public static class StaticFileExtensions
    {
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string path, string directory)
        {
            return UseStaticFiles(builder, options => options.WithRequestPath(path).WithPhysicalPath(directory));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, Action<StaticFileOptions> configuration)
        {
            var options = new StaticFileOptions();
            configuration(options);
            return UseStaticFiles(builder, options);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, StaticFileOptions options)
        {
            return builder
                .UseSendFileFallback()
                .Use(typeof(StaticFileMiddleware), options);
        }
    }
}
