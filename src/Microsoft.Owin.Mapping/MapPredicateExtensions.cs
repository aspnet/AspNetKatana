// <copyright file="MapPredicateExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Mapping;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using Predicate = Func<IDictionary<string, object>, bool>;
    using PredicateAsync = Func<IDictionary<string, object>, Task<bool>>;

    /// <summary>
    /// Extension methods for the MapPredicateMiddleware
    /// </summary>
    public static class MapPredicateExtensions
    {
        /// <summary>
        /// Branches the request pipeline based on the result of the given predicate.
        /// </summary>
        /// <typeparam name="TApp">The application signature</typeparam>
        /// <param name="builder"></param>
        /// <param name="predicate">Invoked with the request environment to determine if the branch should be taken</param>
        /// <param name="branchApp">The branch to take if the predicate Func returns true</param>
        /// <returns></returns>
        public static IAppBuilder MapPredicate<TApp>(this IAppBuilder builder, Predicate predicate, TApp branchApp)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (branchApp == null)
            {
                throw new ArgumentNullException("branchApp");
            }

            IAppBuilder branchBuilder = builder.New();
            branchBuilder.Use(new Func<TApp, TApp>(ignored => branchApp));
            return builder.Use(typeof(MapPredicateMiddleware), branchBuilder.Build(typeof(AppFunc)), predicate);
        }

        /// <summary>
        /// Branches the request pipeline based on the result of the given predicate.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="predicate">Invoked with the request environment to determine if the branch should be taken</param>
        /// <param name="branchConfig">Configures a branch to take</param>
        /// <returns></returns>
        public static IAppBuilder MapPredicate(this IAppBuilder builder, Predicate predicate, Action<IAppBuilder> branchConfig)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (branchConfig == null)
            {
                throw new ArgumentNullException("branchConfig");
            }

            IAppBuilder branchBuilder = builder.New();
            branchConfig(branchBuilder);
            return builder.Use(typeof(MapPredicateMiddleware), branchBuilder.Build(typeof(AppFunc)), predicate);
        }

        /// <summary>
        /// Branches the request pipeline based on the async result of the given predicate.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="predicate">Invoked asynchronously with the request environment to determine if the branch should be taken</param>
        /// <param name="branchApp">The branch to take if the predicate Func returns true</param>
        /// <returns></returns>
        public static IAppBuilder MapPredicateAsync<TApp>(this IAppBuilder builder, PredicateAsync predicate, TApp branchApp)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (branchApp == null)
            {
                throw new ArgumentNullException("branchApp");
            }

            IAppBuilder branchBuilder = builder.New();
            branchBuilder.Use(new Func<TApp, TApp>(ignored => branchApp));
            return builder.Use(typeof(MapPredicateMiddleware), branchBuilder.Build(typeof(AppFunc)), predicate);
        }

        /// <summary>
        /// Branches the request pipeline based on the async result of the given predicate.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="predicate">Invoked asynchronously with the request environment to determine if the branch should be taken</param>
        /// <param name="branchConfig">Configures a branch to take</param>
        /// <returns></returns>
        public static IAppBuilder MapPredicateAsync(this IAppBuilder builder, PredicateAsync predicate, Action<IAppBuilder> branchConfig)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (branchConfig == null)
            {
                throw new ArgumentNullException("branchConfig");
            }

            IAppBuilder branchBuilder = builder.New();
            branchConfig(branchBuilder);
            return builder.Use(typeof(MapPredicateMiddleware), branchBuilder.Build(typeof(AppFunc)), predicate);
        }
    }
}
