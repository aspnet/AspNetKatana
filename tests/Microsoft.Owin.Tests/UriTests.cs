// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
            IOwinRequest request = CreateRequest(pathBase, path, query);
            Assert.Equal(expected, request.Uri.AbsoluteUri);
        }

        private IOwinRequest CreateRequest(string pathBase, string path, string query)
        {
            IOwinRequest request = new OwinRequest();
            request.Scheme = Uri.UriSchemeHttp;
            request.Host = new HostString("host:1");
            request.PathBase = new PathString(pathBase);
            request.Path = new PathString(path);
            request.QueryString = new QueryString(query);
            return request;
        }
    }
}
