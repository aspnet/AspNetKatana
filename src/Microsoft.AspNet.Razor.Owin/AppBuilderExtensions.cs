// <copyright file="AppBuilderExtensions.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using Microsoft.AspNet.Razor.Owin;
using Microsoft.AspNet.Razor.Owin.Compilation;
using Microsoft.AspNet.Razor.Owin.Execution;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.AspNet.Razor.Owin.Routing;

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
