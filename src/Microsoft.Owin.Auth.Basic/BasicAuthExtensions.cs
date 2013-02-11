// <copyright file="BasicAuthExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Auth.Basic;

namespace Owin
{
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string /*user*/, string /*psw*/, Task<bool>>;

    /// <summary>
    /// Extension methods for the BasicAuthMiddleware
    /// </summary>
    public static class BasicAuthExtensions
    {
        /// <summary>
        /// Enable Basic authentication
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">Authentication options</param>
        /// <returns></returns>
        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, BasicAuthOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            return builder.Use(typeof(BasicAuthMiddleware), options);
        }

        /// <summary>
        /// Enable Basic authentication
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authenticate">The user validation callback</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, Func<string, string, Task<bool>> authenticate)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (authenticate == null)
            {
                throw new ArgumentNullException("authenticate");
            }

            var options = new BasicAuthOptions((env, user, pass) => authenticate(user, pass));
            return builder.Use(typeof(BasicAuthMiddleware), options);
        }

        /// <summary>
        /// Enable Basic authentication
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authenticate">The user validation callback</param>
        /// <param name="config">Other authentication options</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, Func<string, string, Task<bool>> authenticate, Action<BasicAuthOptions> config)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (authenticate == null)
            {
                throw new ArgumentNullException("authenticate");
            }
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var options = new BasicAuthOptions((env, user, pass) => authenticate(user, pass));
            config(options);
            return builder.Use(typeof(BasicAuthMiddleware), options);
        }

        /// <summary>
        /// Enable Basic authentication
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authenticate">The user validation callback</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, AuthCallback authenticate)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var options = new BasicAuthOptions(authenticate);
            return builder.Use(typeof(BasicAuthMiddleware), options);
        }

        /// <summary>
        /// Enable Basic authentication
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="authenticate">The user validation callback</param>
        /// <param name="config">Other authentication options</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, AuthCallback authenticate, Action<BasicAuthOptions> config)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var options = new BasicAuthOptions(authenticate);
            config(options);
            return builder.Use(typeof(BasicAuthMiddleware), options);
        }
    }
}
