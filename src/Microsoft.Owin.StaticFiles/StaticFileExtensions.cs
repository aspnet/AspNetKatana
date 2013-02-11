// <copyright file="StaticFileExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.StaticFiles;

namespace Owin
{
    /// <summary>
    /// Extension methods for the StaticFileMiddleware
    /// </summary>
    public static class StaticFileExtensions
    {
        /// <summary>
        /// Enables static file serving for the current request path from the current directory
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder)
        {
            return UseStaticFiles(builder, new StaticFileOptions());
        }

        /// <summary>
        /// Enables static file serving for the current request path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="directory">The physical directory</param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string directory)
        {
            return UseStaticFiles(builder, new StaticFileOptions().WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enables static file serving for the given request path from the given directory
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path">The request path</param>
        /// <param name="directory">The physical directory</param>
        /// <returns></returns>
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, string path, string directory)
        {
            return UseStaticFiles(builder, new StaticFileOptions().WithRequestPath(path).WithPhysicalPath(directory));
        }

        /// <summary>
        /// Enables static file serving with the given options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">The configuration callback</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, Action<StaticFileOptions> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var options = new StaticFileOptions();
            configuration(options);
            return UseStaticFiles(builder, options);
        }

        /// <summary>
        /// Enables static file serving with the given options
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseStaticFiles(this IAppBuilder builder, StaticFileOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            return builder.Use(typeof(StaticFileMiddleware), options);
        }
    }
}
