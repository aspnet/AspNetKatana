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

#if NET45

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;

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
        public static Task Authenticate(this HttpContextBase context, string[] authenticationTypes, Action<IIdentity,
            IDictionary<string, string>, IDictionary<string, object>, object> callback, object state)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinContext owinContext = GetOwinContext(context);
            return owinContext.Authentication.Authenticate(authenticationTypes, callback, state);
        }

        public static IEnumerable<AuthenticationDescription> GetAuthenticationTypes(this HttpContextBase context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinContext owinContext = GetOwinContext(context);
            return owinContext.Authentication.GetAuthenticationTypes();
        }

        public static IEnumerable<AuthenticationDescription> GetAuthenticationTypes(this HttpContextBase context,
            Func<AuthenticationDescription, bool> predicate)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinContext owinContext = GetOwinContext(context);
            return owinContext.Authentication.GetAuthenticationTypes(predicate);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="principal"></param>
        public static void SignIn(this HttpContextBase context, ClaimsIdentity identity)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinContext owinContext = GetOwinContext(context);
            owinContext.Authentication.SignIn(new AuthenticationExtra(), identity);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="principal"></param>
        /// <param name="extra"></param>
        public static void SignIn(this HttpContextBase context, ClaimsIdentity identity, AuthenticationExtra extra)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinContext owinContext = GetOwinContext(context);
            owinContext.Authentication.SignIn(extra, identity);
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

            IOwinContext owinContext = GetOwinContext(context);
            owinContext.Authentication.SignOut(authenticationTypes);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationType"></param>
        public static void Challenge(this HttpContextBase context, string authenticationType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinContext owinContext = GetOwinContext(context);
            owinContext.Authentication.Challenge(new AuthenticationExtra(), authenticationType);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationType"></param>
        /// <param name="extra"></param>
        public static void Challenge(this HttpContextBase context, AuthenticationExtra extra, string authenticationType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinContext owinContext = GetOwinContext(context);
            owinContext.Authentication.Challenge(extra, authenticationType);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="extra"></param>
        /// <param name="authenticationTypes"></param>
        public static void Challenge(this HttpContextBase context, AuthenticationExtra extra, params string[] authenticationTypes)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (extra == null)
            {
                throw new ArgumentNullException("extra");
            }

            IOwinContext owinContext = GetOwinContext(context);
            owinContext.Authentication.Challenge(extra, authenticationTypes);
        }

        private static IDictionary<string, object> GetOwinEnvironment(this HttpContextBase context)
        {
            return (IDictionary<string, object>)context.Items[HttpContextItemKeys.OwinEnvironmentKey];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IOwinContext GetOwinContext(this HttpContextBase context)
        {
            IDictionary<string, object> environment = GetOwinEnvironment(context);

            if (environment == null)
            {
                throw new InvalidOperationException(
                    Microsoft.Owin.Host.SystemWeb.Resources.HttpContext_OwinEnvironmentNotFound);
            }

            return new OwinContext(environment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IOwinContext GetOwinContext(this HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return request.RequestContext.HttpContext.GetOwinContext();
        }
    }
}

#endif
