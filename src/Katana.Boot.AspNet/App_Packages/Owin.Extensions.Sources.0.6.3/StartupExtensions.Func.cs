// Licensed to Monkey Square, Inc. under one or more contributor 
// license agreements.  See the NOTICE file distributed with 
// this work or additional information regarding copyright 
// ownership.  Monkey Square, Inc. licenses this file to you 
// under the Apache License, Version 2.0 (the "License"); you 
// may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal static partial class StartupExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<TApp>(this IAppBuilder builder, Func<TApp, TApp> middleware)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(middleware);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc(this IAppBuilder builder, Func<AppFunc, AppFunc> middleware)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(middleware);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1>(this IAppBuilder builder, Func<AppFunc, T1, AppFunc> middleware, T1 arg1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2>(this IAppBuilder builder, Func<AppFunc, T1, T2, AppFunc> middleware, T1 arg1, T2 arg2)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3>(this IAppBuilder builder, Func<AppFunc, T1, T2, T3, AppFunc> middleware, T1 arg1, T2 arg2, T3 arg3)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2, arg3));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3, T4>(this IAppBuilder builder, Func<AppFunc, T1, T2, T3, T4, AppFunc> middleware, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2, arg3, arg4));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1>(this IAppBuilder builder, Func<T1, Func<AppFunc, AppFunc>> middleware, T1 arg1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1)(app));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2>(this IAppBuilder builder, Func<T1, T2, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2)(app));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3>(this IAppBuilder builder, Func<T1, T2, T3, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2, T3 arg3)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2, arg3)(app));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3, T4>(this IAppBuilder builder, Func<T1, T2, T3, T4, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2, arg3, arg4)(app));
        }
    }
}
