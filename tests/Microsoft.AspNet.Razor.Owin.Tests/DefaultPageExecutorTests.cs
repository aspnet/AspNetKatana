// <copyright file="DefaultPageExecutorTests.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin.Execution;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public class DefaultPageExecutorTests
    {
        public class TheExecuteMethod
        {
            [Fact]
            public void RequiresNonNullPage()
            {
                ContractAssert.NotNull(() => new DefaultPageExecutor().Execute(
                    null,
                    new Request(),
                    NullTrace.Instance), "page");
            }

            [Fact]
            public void RequiresNonNullTracer()
            {
                ContractAssert.NotNull(() => new DefaultPageExecutor().Execute(
                    new Mock<IRazorPage>().Object,
                    new Request(),
                    null), "tracer");
            }

            [Fact]
            public async Task Returns200ResponseAndExecutesPage()
            {
                // Arrange
                var page = new Mock<IRazorPage>();
                var executor = new DefaultPageExecutor();
                Request request = TestData.CreateRequest(path: "/Bar");

                page.Setup(p => p.Run(It.IsAny<Request>(), It.IsAny<Response>()))
                    .Returns((Request req, Response res) =>
                    {
                        res.StatusCode = 200;
                        res.ReasonPhrase = "All good bro";
                        return Task.FromResult(new object());
                    });

                // Act
                await executor.Execute(page.Object, request, NullTrace.Instance);

                var response = new Response(request.Environment);

                // Assert
                page.Verify(p => p.Run(request, response));
                Assert.Equal(200, response.StatusCode);
                Assert.Equal("All good bro", response.ReasonPhrase);
            }
        }
    }
}
