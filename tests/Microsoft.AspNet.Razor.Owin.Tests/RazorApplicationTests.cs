// -----------------------------------------------------------------------
// <copyright file="RazorApplicationTests.cs" company="Microsoft">
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
using Microsoft.AspNet.Razor.Owin.Compilation;
using Microsoft.AspNet.Razor.Owin.Execution;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.AspNet.Razor.Owin.Routing;
using Moq;
using Owin;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RazorApplicationTests
    {
        private static TestableEdgeApplication CreateEdgeApp(AppFunc next, string virtualRoot = "/")
        {
            return new TestableEdgeApplication(next, virtualRoot);
        }

        public class TheConstructor
        {
            [Fact]
            public void RequiresNonNullFileSystem()
            {
                ContractAssert.NotNull(() => new RazorApplication(
                    null,
                    null,
                    "Foo",
                    new Mock<IRouter>().Object,
                    new Mock<ICompilationManager>().Object,
                    new Mock<IPageActivator>().Object,
                    new Mock<IPageExecutor>().Object,
                    new Mock<ITraceFactory>().Object), "fileSystem");
            }

            [Fact]
            public void RequiresNonNullOrEmptyVirtualRoot()
            {
                ContractAssert.NotNullOrEmpty(s => new RazorApplication(
                    null,
                    new Mock<IFileSystem>().Object,
                    s,
                    new Mock<IRouter>().Object,
                    new Mock<ICompilationManager>().Object,
                    new Mock<IPageActivator>().Object,
                    new Mock<IPageExecutor>().Object,
                    new Mock<ITraceFactory>().Object), "virtualRoot");
            }

            [Fact]
            public void RequiresNonNullRouter()
            {
                ContractAssert.NotNull(() => new RazorApplication(
                    null,
                    new Mock<IFileSystem>().Object,
                    "Foo",
                    null,
                    new Mock<ICompilationManager>().Object,
                    new Mock<IPageActivator>().Object,
                    new Mock<IPageExecutor>().Object,
                    new Mock<ITraceFactory>().Object), "router");
            }

            [Fact]
            public void RequiresNonNullCompilationManager()
            {
                ContractAssert.NotNull(() => new RazorApplication(
                    null,
                    new Mock<IFileSystem>().Object,
                    "Foo",
                    new Mock<IRouter>().Object,
                    null,
                    new Mock<IPageActivator>().Object,
                    new Mock<IPageExecutor>().Object,
                    new Mock<ITraceFactory>().Object), "compiler");
            }

            [Fact]
            public void RequiresNonNullPageActivator()
            {
                ContractAssert.NotNull(() => new RazorApplication(
                    null,
                    new Mock<IFileSystem>().Object,
                    "Foo",
                    new Mock<IRouter>().Object,
                    new Mock<ICompilationManager>().Object,
                    null,
                    new Mock<IPageExecutor>().Object,
                    new Mock<ITraceFactory>().Object), "activator");
            }

            [Fact]
            public void RequiresNonNullPageExecutor()
            {
                ContractAssert.NotNull(() => new RazorApplication(
                    null,
                    new Mock<IFileSystem>().Object,
                    "Foo",
                    new Mock<IRouter>().Object,
                    new Mock<ICompilationManager>().Object,
                    new Mock<IPageActivator>().Object,
                    null,
                    new Mock<ITraceFactory>().Object), "executor");
            }

            [Fact]
            public void RequiresNonNullTracer()
            {
                ContractAssert.NotNull(() => new RazorApplication(
                    null,
                    new Mock<IFileSystem>().Object,
                    "Foo",
                    new Mock<IRouter>().Object,
                    new Mock<ICompilationManager>().Object,
                    new Mock<IPageActivator>().Object,
                    new Mock<IPageExecutor>().Object,
                    null), "tracer");
            }
        }

        public class TheStartMethod
        {
            [Fact]
            public async Task DelegatesIfIncomingIsNotUnderVirtualRoot()
            {
                // Arrange
                DelegationTracker delegation = new DelegationTracker();

                var app = CreateEdgeApp(delegation.Next, "/Foo");

                // Act
                await app.Invoke(TestData.CreateCallParams(path: "/Bar"));

                // Assert
                Assert.True(delegation.NextWasCalled);
            }

            [Fact]
            public async Task DelegatesIfIncomingIsNotMatchedByARoute()
            {
                // Arrange
                DelegationTracker delegation = new DelegationTracker();

                var app = CreateEdgeApp(delegation.Next);

                // Act
                await app.Invoke(TestData.CreateCallParams(path: "/Bar"));

                // Assert
                Assert.True(delegation.NextWasCalled);
            }

            [Fact]
            public async Task ThrowsCompilationExceptionIfCompilationFails()
            {
                // Arrange
                var app = CreateEdgeApp(null);

                var testFile = app.TestFileSystem.AddTestFile("Bar.cshtml", "Flarg");

                var expected = new List<CompilationMessage>() 
                {
                    new CompilationMessage(MessageLevel.Error, "Yar!"),
                    new CompilationMessage(MessageLevel.Warning, "Gar!", new FileLocation("Blar.cshtml")),
                    new CompilationMessage(MessageLevel.Info, "Far!", new FileLocation("War.cshtml", 10, 12))
                };

                app.MockCompilationManager
                   .Setup(c => c.Compile(testFile, It.IsAny<ITrace>()))
                   .Returns(Task.FromResult(CompilationResult.Failed(null, expected)));

                // Act
                var ex = await AssertEx.Throws<CompilationFailedException>(async () => await app.Invoke(TestData.CreateCallParams(path: "/Bar")));

                // Assert
                Assert.Equal(
                    String.Format(Resources.CompilationFailedException_MessageWithErrorCounts, 1, 1),
                    ex.Message);
                Assert.Equal(
                    expected,
                    ex.Messages);
            }

            [Fact]
            public async Task ThrowsActivationExceptionIfActivationFails()
            {
                // Arrange
                var app = CreateEdgeApp(null);

                var testFile = app.TestFileSystem.AddTestFile("Bar.cshtml", "Flarg");

                Type compiled = typeof(RazorApplicationTests);
                app.MockCompilationManager
                   .Setup(c => c.Compile(testFile, It.IsAny<ITrace>()))
                   .Returns(Task.FromResult(CompilationResult.Successful(null, compiled, Enumerable.Empty<CompilationMessage>())));
                app.MockActivator
                   .Setup(a => a.ActivatePage(compiled, It.IsAny<ITrace>()))
                   .Returns(ActivationResult.Failed());

                // Act
                var ex = await AssertEx.Throws<ActivationFailedException>(async () => await app.Invoke(TestData.CreateCallParams(path: "/Bar")));

                // Assert
                Assert.Equal(
                    String.Format(Resources.ActivationFailedException_DefaultMessage, compiled.AssemblyQualifiedName),
                    ex.Message);
                Assert.Equal(
                    compiled,
                    ex.AttemptedToActivate);
            }

            [Fact]
            public async Task ReturnsResultOfCallingExecutorIfAllPhasesSucceed()
            {
                // Arrange
                var app = CreateEdgeApp(null);

                var testFile = app.TestFileSystem.AddTestFile("Bar.cshtml", "Flarg");

                Type compiled = typeof(RazorApplicationTests);
                Mock<IRazorPage> page = new Mock<IRazorPage>();
                Response resp = new Response(TestData.CreateCallParams(path: "/Bar"))
                {
                    StatusCode = 418,
                    ReasonPhrase = "I'm a teapot"
                };

                app.MockCompilationManager
                   .Setup(c => c.Compile(testFile, It.IsAny<ITrace>()))
                   .Returns(Task.FromResult(CompilationResult.Successful(null, compiled, Enumerable.Empty<CompilationMessage>())));
                app.MockActivator
                   .Setup(a => a.ActivatePage(compiled, It.IsAny<ITrace>()))
                   .Returns(ActivationResult.Successful(page.Object));
                app.MockExecutor
                   .Setup(e => e.Execute(page.Object, It.IsAny<Request>(), It.IsAny<ITrace>()))
                   .Returns(Task.FromResult<object>(null));

                // Act
                await app.Invoke(TestData.CreateCallParams(path: "/Bar"));

                // Assert
                Assert.Equal(418, resp.StatusCode);
                Assert.Equal("I'm a teapot", resp.ReasonPhrase);
            }
        }

        private class TestableEdgeApplication : RazorApplication
        {
            public TestableEdgeApplication(AppFunc next, string virtualRoot)
                : base(next)
            {
                VirtualRoot = virtualRoot;
                FileSystem = TestFileSystem = new TestFileSystem(@"C:\Test");
                Router = new DefaultRouter(TestFileSystem);
                CompilationManager = (MockCompilationManager = new Mock<ICompilationManager>()).Object;
                Activator = (MockActivator = new Mock<IPageActivator>()).Object;
                Executor = (MockExecutor = new Mock<IPageExecutor>()).Object;
                Tracer = NullTraceFactory.Instance;
            }

            public TestFileSystem TestFileSystem { get; private set; }
            public Mock<ICompilationManager> MockCompilationManager { get; private set; }
            public Mock<IPageActivator> MockActivator { get; private set; }
            public Mock<IPageExecutor> MockExecutor { get; private set; }
        }
    }
}
