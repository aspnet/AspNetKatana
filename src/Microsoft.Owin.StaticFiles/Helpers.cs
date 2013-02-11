// <copyright file="Helpers.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading;

namespace Microsoft.Owin.StaticFiles
{
    internal static class Helpers
    {
        internal static bool IsGetOrHeadMethod(IDictionary<string, object> environment)
        {
            var method = (string)environment[Constants.RequestMethod];
            return "GET".Equals(method, StringComparison.OrdinalIgnoreCase)
                || "HEAD".Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsGetMethod(IDictionary<string, object> environment)
        {
            var method = (string)environment[Constants.RequestMethod];
            return "GET".Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        internal static CancellationToken GetCancellationToken(IDictionary<string, object> environment)
        {
            return (CancellationToken)environment[Constants.CallCancelledKey];
        }

        internal static bool PathEndsInSlash(IDictionary<string, object> environment)
        {
            var path = (string)environment[Constants.RequestPathKey];
            return path.EndsWith("/", StringComparison.Ordinal);
        }

        internal static bool TryMatchPath(IDictionary<string, object> environment, string matchUrl, bool forDirectory, out string subpath)
        {
            var path = (string)environment[Constants.RequestPathKey];

            if (forDirectory && (path.Length == 0 || path[path.Length - 1] != '/'))
            {
                path += "/";
            }

            if (path.StartsWith(matchUrl, StringComparison.OrdinalIgnoreCase)
                && (path.Length == matchUrl.Length
                    || path[matchUrl.Length] == '/'
                    || (matchUrl.Length > 0 && matchUrl[matchUrl.Length - 1] == '/')))
            {
                subpath = path.Substring(matchUrl.Length);
                return true;
            }
            subpath = null;
            return false;
        }
    }
}
