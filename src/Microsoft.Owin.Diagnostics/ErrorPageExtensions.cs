// <copyright file="ShowExceptionsExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Diagnostics;

namespace Owin
{
    /// <summary>
    /// IAppBuilder extension methods for the ErrorPageMiddleware.
    /// </summary>
    public static class ErrorPageExtensions
    {
        /// <summary>
        /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseErrorPage(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseErrorPage(new ErrorPageOptions());
        }

        /// <summary>
        /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IAppBuilder UseErrorPage(this IAppBuilder builder, ErrorPageOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            string appMode = new AppProperties(builder.Properties).Get<string>(Constants.HostAppMode);
            bool isDevMode = string.Equals(Constants.DevMode, appMode, StringComparison.Ordinal);
            return builder.Use<ErrorPageMiddleware>(options, isDevMode);
        }
    }
}
