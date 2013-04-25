// <copyright file="AppBuilderExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Owin;

namespace Microsoft.Owin.Extensions
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Attach the given application to the pipeline.  Nothing attached after this point will be executed.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="app"></param>
        public static void Run(this IAppBuilder builder, object app)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            builder.Use(new Func<object, object>(ignored => app));
        }

        /// <summary>
        /// The Build is called at the point when all of the middleware should be chained
        /// together. May be called to build pipeline branches.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static AppFunc Build(this IAppBuilder builder)
        {
            return builder.Build<AppFunc>();
        }

        /// <summary>
        /// The Build is called at the point when all of the middleware should be chained
        /// together. May be called to build pipeline branches.
        /// </summary>
        /// <typeparam name="TApp">The application signature.</typeparam>
        /// <param name="builder"></param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
        public static TApp Build<TApp>(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return (TApp)builder.Build(typeof(TApp));
        }

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
        /// Adds converters for adapting between disparate application signatures.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="conversion"></param>
        public static void AddSignatureConversion(this IAppBuilder builder, Delegate conversion)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            object obj;
            if (!builder.Properties.TryGetValue("builder.AddSignatureConversion", out obj) || !(obj is Action<Delegate>))
            {
                throw new MissingMethodException(builder.GetType().FullName, "AddSignatureConversion");
            }
            ((Action<Delegate>)obj)(conversion);
        }

        public static void AddSignatureConversion<T1, T2>(this IAppBuilder builder, Func<T1, T2> conversion)
        {
            AddSignatureConversion(builder, (Delegate)conversion);
        }

        /// <summary>
        /// Specifies a middleware instance generator of the given type.
        /// </summary>
        /// <typeparam name="TApp">The application signature.</typeparam>
        /// <param name="builder"></param>
        /// <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
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
