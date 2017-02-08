// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using FakeN.Web;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;
using Xunit;

namespace Microsoft.Owin.Host.SystemWeb.Tests.CallHeaders
{
    public class AspNetRequestHeadersTests
    {
        [Fact]
        public void CreateEmptyRequestHeaders_Success()
        {
            var headers = new AspNetRequestHeaders(new FakeHttpRequest());

            Assert.Equal(0, headers.Count);
            Assert.Equal(0, headers.Count());
            foreach (var header in headers)
            {
                // Should be empty
                Assert.True(false);
            }
        }

        [Fact]
        public void AddHeaders_Success()
        {
            var headers = new AspNetRequestHeaders(new FakeHttpRequest());

            headers.Add("content-length", new string[] { "a", "0" });
            Assert.Equal(1, headers.Count);
            headers.Add("custom", new string[] { "ddfs", "adsfa" });
            Assert.Equal(2, headers.Count);

            int count = 0;
            foreach (var header in headers)
            {
                count++;
            }
            Assert.Equal(2, count);
        }
    }
}
