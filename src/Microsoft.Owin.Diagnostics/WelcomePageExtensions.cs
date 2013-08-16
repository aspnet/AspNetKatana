// <copyright file="TestPageExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;

namespace Owin
{
    /// <summary>
    /// IAppBuilder extensions for the WelcomePageMiddleware.
    /// </summary>
    public static class WelcomePageExtensions
    {
        /// <summary>
        /// Adds the WelcomePageMiddleware to the pipeline with the given options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IAppBuilder UseWelcomePage(this IAppBuilder builder, WelcomePageOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(typeof(WelcomePageMiddleware), options);
        }

        /// <summary>
        /// Adds the WelcomePageMiddleware to the pipeline with the given path.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IAppBuilder UseWelcomePage(this IAppBuilder builder, PathString path)
        {
            return UseWelcomePage(builder, new WelcomePageOptions { Path = path });
        }

        /// <summary>
        /// Adds the WelcomePageMiddleware to the pipeline with the given path.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IAppBuilder UseWelcomePage(this IAppBuilder builder, string path)
        {
            return UseWelcomePage(builder, new WelcomePageOptions { Path = new PathString(path) });
        }

        /// <summary>
        /// Adds the WelcomePageMiddleware to the pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseWelcomePage(this IAppBuilder builder)
        {
            return UseWelcomePage(builder, new WelcomePageOptions());
        }
    }
}
