// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Owin;

namespace Microsoft.Owin.Builder.Tests
{
    public static class AppBuilderTestExtensions
    {
        /// <summary>
        /// Creates a new IAppBuilder instance from the current one and then invokes the configuration callback.
        /// </summary>
        /// <typeparam name="TApp">The application signature.</typeparam>
        /// <param name="builder"></param>
        /// <param name="configuration">The callback for configuration.</param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "By design")]
        public static TApp BuildNew<TApp>(this IAppBuilder builder, Action<IAppBuilder> configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            IAppBuilder nested = builder.New();
            configuration(nested);
            return nested.Build<TApp>();
        }

        /// <summary>
        /// Specifies a middleware instance generator of the given type.
        /// </summary>
        /// <typeparam name="TApp">The application signature.</typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware.</param>
        /// <returns></returns>
        public static IAppBuilder UseFunc<TApp>(this IAppBuilder builder, Func<TApp, TApp> middleware)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(middleware);
        }
    }
}
