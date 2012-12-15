// -----------------------------------------------------------------------
// <copyright file="StaticFileExtentions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Owin;

namespace Microsoft.Owin.StaticFiles
{
    public static class StaticFileExtensions
    {
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string path, string directory)
        {
            return builder.UseStaticFiles(new[] { new KeyValuePair<string, string>(path, directory) });
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, IList<KeyValuePair<string, string>> pathsAndDirectories)
        {
            return builder
                .UseDirectoryBrowser(pathsAndDirectories)
                .UseSendFileFallback()
                .UseFileLookup(pathsAndDirectories);
        }
    }
}
