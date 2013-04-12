// <copyright file="OwinResponseExtensions.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
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

using Owin.Types.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;

namespace Owin.Types.Extensions
{
#region OwinResponseExtensions.Cookies

    internal static partial class OwinRequestExtensions
    {
        public static OwinResponse AddCookie(this OwinResponse response, string key, string value)
        {
            OwinHelpers.AddCookie(response, key, value);
            return response;
        }

        public static OwinResponse AddCookie(this OwinResponse response, string key, string value, CookieOptions options)
        {
            OwinHelpers.AddCookie(response, key, value, options);
            return response;
        }

        public static OwinResponse DeleteCookie(this OwinResponse response, string key)
        {
            OwinHelpers.DeleteCookie(response, key);
            return response;
        }

        public static OwinResponse DeleteCookie(this OwinResponse response, string key, CookieOptions options)
        {
            OwinHelpers.DeleteCookie(response, key, options);
            return response;
        }
    }
#endregion

#region OwinResponseExtensions

    [System.CodeDom.Compiler.GeneratedCode("App_Packages", "")]
    internal static partial class OwinResponseExtensions
    {
    }
#endregion

#region OwinResponseExtensions.Http

    internal static partial class OwinResponseExtensions
    {
        public static void Redirect(this OwinResponse response, string location)
        {
            response.StatusCode = 302;
            response.SetHeader("Location", location);
        }
    }
#endregion

#region OwinResponseExtensions.Security

    internal static partial class OwinResponseExtensions
    {
        public static void SignIn(this OwinResponse response, IPrincipal user)
        {
            SignIn(response, user, null);
        }

        public static void SignIn(this OwinResponse response, IPrincipal user, IDictionary<string, object> extra)
        {
            response.SignIn = new Tuple<IPrincipal, IDictionary<string, object>>(user, extra);
        }

        public static void SignOut(this OwinResponse response, params string[] authenticationTypes)
        {
            response.SignOut = authenticationTypes;
        }

        public static void Unauthorized(this OwinResponse response, params string[] authenticationTypes)
        {
            Unauthorized(response, authenticationTypes, null);
        }

        public static void Unauthorized(this OwinResponse response, string[] authenticationTypes, Claim[] claims)
        {
            response.StatusCode = 401;
            response.Challenge = new Tuple<string[], Claim[]>(authenticationTypes, claims);
        }
    }
#endregion

}
