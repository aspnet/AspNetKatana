// <copyright file="DefaultRouterTests.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Linq;
using System.Threading.Tasks;
using Gate;
using Microsoft.AspNet.Razor.Owin.Execution;
using Microsoft.AspNet.Razor.Owin.Routing;
using Microsoft.Owin.FileSystems;
using Moq;
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
                TestableDefaultRouter router = CreateRouter();
                IFileInfo expectedFile = router.TestFileSystem.AddTestFile(path);

                // Act
                RouteResult routed = await router.Route(
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
                TestableDefaultRouter router = CreateRouter();
                IFileInfo expectedFile = router.TestFileSystem.AddTestFile(@"Does\Not\Match");

                // Act
                RouteResult routed = await router.Route(
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
