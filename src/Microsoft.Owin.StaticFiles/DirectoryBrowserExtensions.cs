// -----------------------------------------------------------------------
// <copyright file="DirectoryBrowserExtensions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Owin;

namespace Microsoft.Owin.StaticFiles
{
    public static class DirectoryBrowserExtensions
    {
        public static IAppBuilder UseDirectoryBrowser(this IAppBuilder builder, string path, string directory)
        {
            return builder.UseDirectoryBrowser(new[] { new KeyValuePair<string, string>(path, directory) });
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseDirectoryBrowser(this IAppBuilder builder, IList<KeyValuePair<string, string>> pathsAndDirectories)
        {
            return builder.Use(typeof(DirectoryBrowser), pathsAndDirectories);
        }
    }
}
