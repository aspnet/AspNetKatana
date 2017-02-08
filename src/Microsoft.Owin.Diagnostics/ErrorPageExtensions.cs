// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Logging;

namespace Owin
{
    /// <summary>
    /// IAppBuilder extension methods for the ErrorPageMiddleware.
    /// </summary>
    public static class ErrorPageExtensions
    {
        /// <summary>
        /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
        /// Full error details are only displayed by default if 'host.AppMode' is set to 'development' in the IAppBuilder.Properties.
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
        /// Full error details are only displayed by default if 'host.AppMode' is set to 'development' in the IAppBuilder.Properties.
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
            ILogger logger = builder.CreateLogger<ErrorPageMiddleware>();
            return builder.Use<ErrorPageMiddleware>(options, logger, isDevMode);
        }
    }
}
