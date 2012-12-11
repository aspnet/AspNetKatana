// -----------------------------------------------------------------------
// <copyright file="StaticFileExtentions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Owin;

namespace Microsoft.Owin.StaticFiles
{
    public static class StaticFileExtensions
    {
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string url, string dir)
        {
            return builder.UseStaticFiles(new[] { new KeyValuePair<string, string>(url, dir) });
        }

        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, IList<KeyValuePair<string, string>> urlsAndDirs)
        {
            return builder
                .UseSendFileFallback()
                .Use(typeof(StaticFiles), urlsAndDirs);
        }
    }
}
