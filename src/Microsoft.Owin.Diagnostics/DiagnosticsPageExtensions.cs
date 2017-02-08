// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DEBUG
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// IAppBuilder extensions for the DiagnosticsPageMiddleware.
    /// </summary>
    public static class DiagnosticsPageExtensions
    {
        /// <summary>
        /// Adds the DiagnosticsPageMiddleware to the pipeline with the given options.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IAppBuilder UseDiagnosticsPage(this IAppBuilder builder, DiagnosticsPageOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(typeof(DiagnosticsPageMiddleware), options);
        }

        /// <summary>
        /// Adds the DiagnosticsPageMiddleware to the pipeline with the given path.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IAppBuilder UseDiagnosticsPage(this IAppBuilder builder, PathString path)
        {
            return UseDiagnosticsPage(builder, new DiagnosticsPageOptions { Path = path });
        }

        /// <summary>
        /// Adds the DiagnosticsPageMiddleware to the pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IAppBuilder UseDiagnosticsPage(this IAppBuilder builder)
        {
            return UseDiagnosticsPage(builder, new DiagnosticsPageOptions());
        }
    }
}
#endif