// <copyright file="DefaultFilesExtensions.cs" company="Katana contributors">
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
using System.Collections.Generic;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace Owin
{
    /// <summary>
    /// Extension methods for the DefaultFilesMiddleware
    /// </summary>
    public static class DefaultFilesExtensions
    {
        /// <summary>
        /// Enables default file serving on the current path from the current directory
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions());
        }

        /// <summary>
        /// Enables default file serving on the current path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directory">The physical file system directory</param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string directory)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions().WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enables default file serving on the given path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The request path</param>
        /// <param name="directory">The physical file system directory</param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string path, string directory)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions().WithRequestPath(path).WithPhysicalPath(directory));
        }

        /// <summary>
        /// Serves the given file names by default for the given path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The request path</param>
        /// <param name="directory">The physical file system directory</param>
        /// <param name="defaultFiles">The default file names</param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, string path, string directory, IEnumerable<string> defaultFiles)
        {
            return builder.UseDefaultFiles(new DefaultFilesOptions().WithRequestPath(path).WithPhysicalPath(directory).WithDefaultFileNames(defaultFiles));
        }

        /// <summary>
        /// Enables default file serving with the given options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IAppBuilder UseDefaultFiles(this IAppBuilder builder, DefaultFilesOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(typeof(DefaultFilesMiddleware), options);
        }
    }
}
