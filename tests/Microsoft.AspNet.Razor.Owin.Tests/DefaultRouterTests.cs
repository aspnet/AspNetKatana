// -----------------------------------------------------------------------
// <copyright file="DefaultRouterTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.AspNet.Razor.Owin.Routing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public class DefaultRouterTests
    {
        private static TestableDefaultRouter CreateRouter()
        {
            return new TestableDefaultRouter();
        }

        public class TheConstructor
        {
            [Fact]
            public void RequiresNonNullFileSystem()
            {
                ContractAssert.NotNull(() => new DefaultRouter(null), "fileSystem");
            }

            [Fact]
            public void InitializesFileSystem()
            {
                // Arrange
                IFileSystem expected = new PhysicalFileSystem(@"C:\Root");

                // Act
                var router = new DefaultRouter(expected);

                // Assert
                Assert.Same(expected, router.FileSystem);
            }

            [Fact]
            public void HasBuiltInDefaultDocumentAndKnownExtensions()
            {
                // Act
                var router = new DefaultRouter(new PhysicalFileSystem(@"C:\Root"));

                // Assert
                Assert.Equal(
                    new[] { ".cshtml" },
                    router.KnownExtensions.ToArray());
                Assert.Equal(
                    new[] { "Default", "Index" },
                    router.DefaultDocumentNames.ToArray());
            }
        }

        public class TheRouteMethod
        {
            [Fact]
            public void RequiresNonNullTracer()
            {
                ContractAssert.NotNull(() => new DefaultRouter(new PhysicalFileSystem(@"C:\Root")).Route(new Request(), null), "tracer");
            }

            [Theory]
            [InlineData("/Foo/Bar/Baz", @"Foo\Bar\Baz.cshtml", "")]
            [InlineData("/Foo/Bar", @"Foo\Bar.cshtml", "")]
            [InlineData("/Foo/Bar/Baz", @"Foo\Bar.cshtml", "Baz")]
            [InlineData("/Foo/Bar", @"Foo.cshtml", "Bar")]
            [InlineData("/Foo/Bar/Baz", @"Foo\Bar\Default.cshtml", "Baz")]
            [InlineData("/Foo/Bar", @"Foo\Default.cshtml", "Bar")]
            [InlineData("/Foo/Bar/Baz", @"Foo\Bar\Index.cshtml", "Baz")]
            [InlineData("/Foo/Bar", @"Foo\Index.cshtml", "Bar")]
            [InlineData("/Foo/Bar/Baz", @"Foo\Bar\default.cshtml", "Baz")]
            [InlineData("/Foo/Bar", @"Foo\default.cshtml", "Bar")]
            [InlineData("/Foo/Bar/Baz", @"Foo\Bar\index.cshtml", "Baz")]
            [InlineData("/Foo/Bar", @"Foo\index.cshtml", "Bar")]
            [InlineData("/", @"Index.cshtml", "")]
            [InlineData("/Bar", @"Index.cshtml", "Bar")]
            public async Task SuccessfulRouteTests(string vpath, string path, string pathInfo)
            {
                // Arrange
                var router = CreateRouter();
                var expectedFile = router.TestFileSystem.AddTestFile(path);

                // Act
                var routed = await router.Route(
                    TestData.CreateRequest(path: vpath),
                    NullTrace.Instance);

                // Assert
                Assert.True(routed.Success);
                Assert.Equal(pathInfo, routed.PathInfo);
                Assert.Equal(expectedFile, routed.File);
            }

            [Fact]
            public async Task ReturnsFailedResultIfNoFileMatchesVirtualPath()
            {
                // Arrange
                var router = CreateRouter();
                var expectedFile = router.TestFileSystem.AddTestFile(@"Does\Not\Match");

                // Act
                var routed = await router.Route(
                    TestData.CreateRequest(path: "Does/This/Match"),
                    NullTrace.Instance);

                // Assert
                Assert.False(routed.Success);
                Assert.Null(routed.PathInfo);
                Assert.Null(routed.File);
            }
        }

        private class TestableDefaultRouter : DefaultRouter
        {
            public TestableDefaultRouter()
            {
                FileSystem = (TestFileSystem = new TestFileSystem(@"C:\Root"));
            }

            public TestFileSystem TestFileSystem { get; private set; }
        }
    }
}
