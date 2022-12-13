// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Infrastructure;
using Owin;

namespace Microsoft.Owin
{
    /// <summary>
    /// Cookie-related extensions for <see cref="IAppBuilder"/>.
    /// </summary>
    public static class AppBuilderCookieExtensions
    {
        private const string CookieManagerProperty = "infrastructure.CookieManager";
        private const string ChunkingCookieManagerProperty = "infrastructure.ChunkingCookieManager";

        /// <summary>
        /// Sets the default <see cref="ICookieManager"/> that will be used by
        /// the OWIN middleware when no manager is explicitly set in their options.
        /// </summary>
        /// <remarks>
        /// Depending on the host, a default <see cref="ICookieManager"/> instance
        /// may already be registered in <see cref="IAppBuilder.Properties"/>
        /// (e.g SystemWeb automatically registers a cookie manager that relies on
        /// HttpCookieCollection instead of directly manipulating the HTTP headers).
        /// </remarks>
        /// <param name="app">The application builder.</param>
        /// <param name="manager">The cookie manager.</param>
        /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
        public static void SetDefaultCookieManager(this IAppBuilder app, ICookieManager manager)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (manager is null)
            {
                app.Properties.Remove(CookieManagerProperty);
            }
            else
            {
                app.Properties[CookieManagerProperty] = manager;
            }
        }

        /// <summary>
        /// Sets the default chunking <see cref="ICookieManager"/> that will be used
        /// by the OWIN middleware when no manager is explicitly set in their options.
        /// </summary>
        /// <remarks>
        /// Depending on the host, a default <see cref="ICookieManager"/> instance
        /// may already be registered in <see cref="IAppBuilder.Properties"/>
        /// (e.g SystemWeb automatically registers a cookie manager that relies on
        /// HttpCookieCollection instead of directly manipulating the HTTP headers).
        /// </remarks>
        /// <param name="app">The application builder.</param>
        /// <param name="manager">The cookie manager.</param>
        /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
        public static void SetDefaultChunkingCookieManager(this IAppBuilder app, ICookieManager manager)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (manager is null)
            {
                app.Properties.Remove(ChunkingCookieManagerProperty);
            }
            else
            {
                app.Properties[ChunkingCookieManagerProperty] = manager;
            }
        }

        /// <summary>
        /// Gets the default <see cref="ICookieManager"/> that will be used by
        /// the OWIN middleware when no manager is explicitly set in their options.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>
        /// The cookie manager or a default <see cref="CookieManager"/> instance if no manager was registered.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
        public static ICookieManager GetDefaultCookieManager(this IAppBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Properties.TryGetValue(CookieManagerProperty, out object value) &&
                value is ICookieManager manager ? manager : new CookieManager();
        }

        /// <summary>
        /// Gets the default chunking <see cref="ICookieManager"/> that will be used
        /// by the OWIN middleware when no manager is explicitly set in their options.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>
        /// The cookie manager or a default <see cref="ChunkingCookieManager"/> instance if no manager was registered.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="app"/> is <see langword="null"/>.</exception>
        public static ICookieManager GetDefaultChunkingCookieManager(this IAppBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Properties.TryGetValue(ChunkingCookieManagerProperty, out object value) &&
                value is ICookieManager manager ? manager : new ChunkingCookieManager();
        }
    }
}
