// <copyright file="HttpContextBaseExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Owin.Types;
using Owin.Types.Extensions;

namespace System.Web
{
    /// <summary>Provides extension methods for <see cref="HttpContextBase"/>.</summary>
    public static partial class HttpContextBaseExtensions
    {
        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationTypes"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Following Owin conventions.")]
        public static Task Authenticate(this HttpContextBase context, string[] authenticationTypes, Action<IIdentity,
            IDictionary<string, string>, IDictionary<string, object>, object> callback, object state)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            OwinRequest request = GetOwinRequest(context);
            return request.Authenticate(authenticationTypes, callback, state);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Following Owin conventions.")]
        public static Task GetAuthenticationTypes(this HttpContextBase context,
            Action<IDictionary<string, object>, object> callback, object state)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            OwinRequest request = GetOwinRequest(context);
            return request.GetAuthenticationTypes(callback, state);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        public static void SignIn(this HttpContextBase context, IPrincipal user)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            OwinResponse response = GetOwinResponse(context);
            response.SignIn(user);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <param name="extra"></param>
        public static void SignIn(this HttpContextBase context, IPrincipal user, IDictionary<string, string> extra)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            OwinResponse response = GetOwinResponse(context);
            response.SignIn(user, extra);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationTypes"></param>
        public static void SignOut(this HttpContextBase context, params string[] authenticationTypes)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            OwinResponse response = GetOwinResponse(context);
            response.SignOut(authenticationTypes);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationTypes"></param>
        public static void Challenge(this HttpContextBase context, params string[] authenticationTypes)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            OwinResponse response = GetOwinResponse(context);
            response.Unauthorized(authenticationTypes);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationTypes"></param>
        /// <param name="extra"></param>
        public static void Challenge(this HttpContextBase context, string[] authenticationTypes, IDictionary<string, string> extra)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            OwinResponse response = GetOwinResponse(context);
            response.Unauthorized(authenticationTypes, extra);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationTypes"></param>
        /// <param name="extra"></param>
        public static void Challenge(this HttpContextBase context, string[] authenticationTypes, AuthenticationExtra extra)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (extra == null)
            {
                throw new ArgumentNullException("extra");
            }

            OwinResponse response = GetOwinResponse(context);
            response.Unauthorized(authenticationTypes, extra.Properties);
        }

        private static IDictionary<string, object> GetOwinEnvironment(this HttpContextBase context)
        {
            return (IDictionary<string, object>)context.Items[HttpContextItemKeys.OwinEnvironmentKey];
        }

        private static OwinResponse GetOwinResponse(this HttpContextBase context)
        {
            IDictionary<string, object> environment = GetOwinEnvironment(context);

            if (environment == null)
            {
                throw new InvalidOperationException(
                    Microsoft.Owin.Host.SystemWeb.Resources.HttpContext_OwinEnvironmentNotFound);
            }

            return new OwinResponse(environment);
        }

        private static OwinRequest GetOwinRequest(this HttpContextBase context)
        {
            IDictionary<string, object> environment = GetOwinEnvironment(context);

            if (environment == null)
            {
                throw new InvalidOperationException(
                    Microsoft.Owin.Host.SystemWeb.Resources.HttpContext_OwinEnvironmentNotFound);
            }

            return new OwinRequest(environment);
        }
    }
}
