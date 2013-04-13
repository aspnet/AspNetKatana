// <copyright file="OwinRequestExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Owin.Types.Extensions
{
#region OwinRequestExtensions.Cookies

    internal static partial class OwinRequestExtensions
    {
        public static IDictionary<string, string> GetCookies(this OwinRequest request)
        {
            return OwinHelpers.GetCookies(request);
        }
    }
#endregion

#region OwinRequestExtensions

    [System.CodeDom.Compiler.GeneratedCode("App_Packages", "")]
    internal static partial class OwinRequestExtensions
    {
    }
#endregion

#region OwinRequestExtensions.Forwarded.

    internal static partial class OwinRequestExtensions
    {
        public static string GetForwardedScheme(this OwinRequest request)
        {
            return OwinHelpers.GetForwardedScheme(request);
        }

        public static string GetForwardedHost(this OwinRequest request)
        {
            return OwinHelpers.GetForwardedHost(request);
        }

        public static Uri GetForwardedUri(this OwinRequest request)
        {
            return OwinHelpers.GetForwardedUri(request);
        }
        
        public static OwinRequest ApplyForwardedScheme(this OwinRequest request)
        {
            return OwinHelpers.ApplyForwardedScheme(request);
        }
        
        public static OwinRequest ApplyForwardedHost(this OwinRequest request)
        {
            return OwinHelpers.ApplyForwardedHost(request);
        }
        
        public static OwinRequest ApplyForwardedUri(this OwinRequest request)
        {
            return OwinHelpers.ApplyForwardedUri(request);
        }
    }
#endregion

#region OwinRequestExtensions.MethodOverride

    internal static partial class OwinRequestExtensions
    {
        public static string GetMethodOverride(this OwinRequest request)
        {
            return OwinHelpers.GetMethodOverride(request);
        }

        public static OwinRequest ApplyMethodOverride(this OwinRequest request)
        {
            return OwinHelpers.ApplyMethodOverride(request);
        }
    }
#endregion

#region OwinRequestExtensions.Query

    internal static partial class OwinRequestExtensions
    {
        public static IDictionary<string, string[]> GetQuery(this OwinRequest request)
        {
            return OwinHelpers.GetQuery(request);
        }
    }
#endregion

}
