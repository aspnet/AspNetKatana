// <copyright file="AspNetRequestHeaders.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.CallHeaders
{
    // TODO: Implement a proper pass through wrapper collection.
    internal static class AspNetRequestHeaders
    {
        internal static IDictionary<string, string[]> Create(HttpRequestBase httpRequest)
        {
            // PERF: this method will return an IDictionary facade to enable two things...
            //   direct enumeration from original headers if only GetEnumerator is called,
            //   readonly responses for a few operations from original namevaluecollection,
            //   just-in-time creation and pass-through to real Dictionary for other calls
            return httpRequest.Headers.AllKeys.ToDictionary(
                key => key,
                key => (string[])httpRequest.Headers.GetValues(key),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
