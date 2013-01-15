// -----------------------------------------------------------------------
// <copyright file="DefaultPageExecutorTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin;
using Microsoft.AspNet.Razor.Owin.Execution;
using Moq;
using Owin;
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
                var request = TestData.CreateRequest(path: "/Bar");

                page.Setup(p => p.Run(It.IsAny<Request>(), It.IsAny<Response>()))
                    .Returns((Request req, Response res) =>
                    {
                        res.StatusCode = 200;
                        res.ReasonPhrase = "All good bro";
                        return Task.FromResult(new object());
                    });

                // Act
                await executor.Execute(page.Object, request, NullTrace.Instance);

                Response response = new Response(request.Environment);

                // Assert
                page.Verify(p => p.Run(request, response));
                Assert.Equal(200, response.StatusCode);
                Assert.Equal("All good bro", response.ReasonPhrase);
            }
        }
    }
}
