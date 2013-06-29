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

// These are less common extensions so we don't want them in the Owin namespace.
namespace Microsoft.Owin.Builder
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Extension methods for IAppBuilder.
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// The Build is called at the point when all of the middleware should be chained
        /// together. May be called to build pipeline branches.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>The request processing entry point for this section of the pipeline.</returns>
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
            if (builder.Properties.TryGetValue("builder.AddSignatureConversion", out obj))
            {
                Action<Delegate> action = obj as Action<Delegate>;
                if (action != null)
                {
                    action(conversion);
                    return;
                }
            }
            throw new MissingMethodException(builder.GetType().FullName, "AddSignatureConversion");
        }

        /// <summary>
        /// Adds converters for adapting between disparate application signatures.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="builder"></param>
        /// <param name="conversion"></param>
        public static void AddSignatureConversion<T1, T2>(this IAppBuilder builder, Func<T1, T2> conversion)
        {
            AddSignatureConversion(builder, (Delegate)conversion);
        }
    }
}
