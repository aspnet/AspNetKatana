// <copyright file="AspNetRequestHeadersTests.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Specialized;
using System.Linq;
using FakeN.Web;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;
using Xunit;

#if NET40
namespace Microsoft.Owin.Host.SystemWeb.Tests.CallHeaders
#else

namespace Microsoft.Owin.Host.SystemWeb.Tests45.CallHeaders
#endif
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
