// <copyright file="TestData.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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

using System.Collections.Generic;
using Gate;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    internal static class TestData
    {
        public static IDictionary<string, object> CreateCallParams(
            string method = "GET",
            string path = "/",
            string pathBase = "",
            string queryString = "",
            string scheme = "http",
            string version = "1.0")
        {
            var cp = new Dictionary<string, object>()
            {
                { "owin.RequestMethod", method },
                { "owin.RequestPath", path },
                { "owin.RequestPathBase", pathBase },
                { "owin.RequestQueryString", queryString },
                { "owin.RequestScheme", scheme },
                { "owin.Version", version }
            };
            return cp;
        }

        public static Request CreateRequest(
            string method = "GET",
            string path = "/",
            string pathBase = "",
            string queryString = "",
            string scheme = "http",
            string version = "1.0")
        {
            return new Request(CreateCallParams(
                method, path, pathBase, queryString, scheme, version));
        }

        public static TestFile CreateDummyFile()
        {
            return new TestFile("Irrel", "evan", "t");
        }
    }
}
