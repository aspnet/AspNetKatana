// <copyright file="UriTests.cs" company="Microsoft Open Technologies, Inc.">
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
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Tests
{
    public class UriTests
    {
        [Theory]
        [InlineData("", "", "", "http://host:1/")]
        [InlineData("", "/", "", "http://host:1/")]
        [InlineData("/", "", "", "http://host:1/")]
        [InlineData("", "", "a", "http://host:1/?a")]
        [InlineData("", "/", "a", "http://host:1/?a")]
        // [InlineData("", "/a\\b", "a", "http://host:1/a/b?a")] // .NET 4.0 un-escapes \ and converts to /, .NET 4.5 does not.
        [InlineData("", "/葉", "葉", "http://host:1/%E8%91%89?%E8%91%89")]
        [InlineData("", "/", "a#b", "http://host:1/?a%23b")]
        // System.Uri would trim trailing spaces, escape them if you want them.
        [InlineData("", "/ ", " ", "http://host:1/%20")]
        [InlineData("/a%.+#?", "/z", "a#b", "http://host:1/a%25.%2B%23%3F/z?a%23b")]
        // Note: Http.Sys will not accept any characters in the path that it cannot un-escape,
        // so this double escaping is not a problem in production.
        [InlineData("", "/%20", "%20", "http://host:1/%2520?%20")]
        public void UriReconstruction(string pathBase, string path, string query, string expected)
        {
            OwinRequest request = CreateRequest(pathBase, path, query);
            Assert.Equal(expected, request.Uri.AbsoluteUri);
        }

        private OwinRequest CreateRequest(string pathBase, string path, string query)
        {
            OwinRequest request = new OwinRequest(new Dictionary<string, object>());
            request.Scheme = Uri.UriSchemeHttp;
            request.Host = "host:1";
            request.PathBase = pathBase;
            request.Path = path;
            request.QueryString = query;
            return request;
        }
    }
}
