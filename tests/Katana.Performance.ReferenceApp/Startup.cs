// <copyright file="Startup.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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
