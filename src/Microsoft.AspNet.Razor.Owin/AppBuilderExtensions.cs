// -----------------------------------------------------------------------
// <copyright file="RazorExtensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gate;
using Gate.Middleware;
using Microsoft.AspNet.Razor.Owin;
using Microsoft.AspNet.Razor.Owin.Compilation;
using Microsoft.AspNet.Razor.Owin.Execution;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.AspNet.Razor.Owin.Routing;
using Owin;

namespace Owin
{
    public static class RazorExtensions
    {
        public static IAppBuilder UseRazor(this IAppBuilder builder)
        {
            Requires.NotNull(builder, "builder");

            return UseRazor(builder, new PhysicalFileSystem(Environment.CurrentDirectory), "/");
        }

        public static IAppBuilder UseRazor(this IAppBuilder builder, string rootDirectory)
        {
            Requires.NotNull(builder, "builder");
            Requires.NotNullOrEmpty(rootDirectory, "rootDirectory");

            return UseRazor(builder, new PhysicalFileSystem(rootDirectory), "/");
        }

        public static IAppBuilder UseRazor(this IAppBuilder builder, string rootDirectory, string virtualRoot)
        {
            Requires.NotNull(builder, "builder");
            Requires.NotNullOrEmpty(rootDirectory, "rootDirectory");
            Requires.NotNullOrEmpty(virtualRoot, "virtualRoot");

            return UseRazor(builder, new PhysicalFileSystem(rootDirectory), virtualRoot);
        }

        public static IAppBuilder UseRazor(this IAppBuilder builder, IFileSystem fileSystem)
        {
            Requires.NotNull(builder, "builder");
            Requires.NotNull(fileSystem, "fileSystem");
            
            return UseRazor(builder, fileSystem, "/");
        }

        public static IAppBuilder UseRazor(this IAppBuilder builder, IFileSystem fileSystem, string virtualRoot)
        {
            Requires.NotNull(builder, "builder");
            Requires.NotNull(fileSystem, "fileSystem");
            Requires.NotNullOrEmpty(virtualRoot, "virtualRoot");
            
            return builder.Use(typeof(RazorApplication),
                fileSystem,
                virtualRoot,
                new DefaultRouter(fileSystem),
                new DefaultCompilationManager(new TimestampContentIdentifier()),
                new DefaultPageActivator(),
                new DefaultPageExecutor(),
                new GateTraceFactory());
        }
    }
}
