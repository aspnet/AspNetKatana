// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Katana.Performance.ReferenceApp;
using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Katana.Performance.ReferenceApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            /* // Note: Enable only for debugging. This slows down the perf tests.
            app.Use((context, next) =>
            {
                var req = context.Request;
                context.TraceOutput.WriteLine("{0} {1}{2} {3}", req.Method, req.PathBase, req.Path, req.QueryString);
                return next();
            });*/

            app.UseErrorPage(new ErrorPageOptions { SourceCodeLineCount = 20 });
            // app.Use(typeof(AutoTuneMiddleware), app.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"]);
            app.UseSendFileFallback();
            app.Use<CanonicalRequestPatterns>();

            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = new PathString("/static"),
                FileSystem = new PhysicalFileSystem("public")
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                RequestPath = new PathString("/static"),
                FileSystem = new PhysicalFileSystem("public")
            });
            app.UseStageMarker(PipelineStage.MapHandler);

            FileServerOptions options = new FileServerOptions();
            options.EnableDirectoryBrowsing = true;
            options.StaticFileOptions.ServeUnknownFileTypes = true;

            app.UseWelcomePage("/Welcome");
        }
    }
}
