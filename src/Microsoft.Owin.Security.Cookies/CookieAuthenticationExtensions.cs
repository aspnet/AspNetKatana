// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;

namespace Owin
{
    /// <summary>
    /// Extension methods provided by the cookies authentication middleware
    /// </summary>
    public static class CookieAuthenticationExtensions
    {
        /// <summary>
        /// Adds a cookie-based authentication middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method</param>
        /// <param name="options">An options class that controls the middleware behavior</param>
        /// <returns>The original app parameter</returns>
        public static IAppBuilder UseCookieAuthentication(this IAppBuilder app, CookieAuthenticationOptions options)
        {
            return app.UseCookieAuthentication(options, PipelineStage.Authenticate);
        }

        /// <summary>
        /// Adds a cookie-based authentication middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method</param>
        /// <param name="options">An options class that controls the middleware behavior</param>
        /// <param name="stage"></param>
        /// <returns>The original app parameter</returns>
        public static IAppBuilder UseCookieAuthentication(this IAppBuilder app, CookieAuthenticationOptions options, PipelineStage stage)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            app.Use(typeof(CookieAuthenticationMiddleware), app, options);
            app.UseStageMarker(stage);
            return app;
        }
    }
}
