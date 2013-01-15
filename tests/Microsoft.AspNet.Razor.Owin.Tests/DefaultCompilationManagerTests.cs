// -----------------------------------------------------------------------
// <copyright file="DefaultCompilationManagerTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Owin;
using Microsoft.AspNet.Razor.Owin.Compilation;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    public class DefaultCompilationManagerTests
    {
        public static TestableDefaultCompilationManager CreateManager()
        {
            return new TestableDefaultCompilationManager();
        }

        public class TheConstructor
        {
            [Fact]
            public void RequiresNonNullContentIdentifier()
            {
                ContractAssert.NotNull(() => new DefaultCompilationManager(null), "identifier");
            }

            [Fact]
            public void InitializesProperties()
            {
                // Arrange
                var contentIder = new Mock<IContentIdentifier>().Object;

                // Act
                var cm = new DefaultCompilationManager(contentIder);

                // Assert
                Assert.Same(contentIder, cm.ContentIdentifier);
                Assert.IsType<RazorCompiler>(cm.Compilers.Single());
            }
        }

        public class TheCompileMethod
        {
            [Fact]
            public void RequiresNonNullFile()
            {
                ContractAssert.NotNull(() => CreateManager().Compile(null, NullTrace.Instance), "file");
            }

            [Fact]
            public void RequiresNonNullTracer()
            {
                ContractAssert.NotNull(() => CreateManager().Compile(new TestFile("a", "file"), null), "tracer");
            }

            [Fact]
            public async Task ReturnsCacheIfHitAndReferenceValid()
            {
                // Arrange
                var cached = new Mock<Type>();
                var cm = CreateManager();
                var file = TestData.CreateDummyFile();
                cm.Cache["Foo"] = new WeakReference<Type>(cached.Object);
                cm.MockContentIdentifier
                  .Setup(i => i.GenerateContentId(file)).Returns("Foo");
                
                // Act
                var result = await cm.Compile(file, NullTrace.Instance);

                // Assert
                Assert.True(result.Success);
                Assert.True(result.SatisfiedFromCache);
                Assert.Equal(0, result.Messages.Count);
                Assert.Same(cached.Object, result.GetCompiledType());
            }

            [Fact]
            public async Task ReturnsAndCachesCompiledResultIfCacheMisses()
            {
                // Arrange
                var compiled = new Mock<Type>();
                var cm = CreateManager();
                var file = TestData.CreateDummyFile();
                var compiler = new Mock<ICompiler>();
                cm.Compilers.Add(compiler.Object);
                cm.MockContentIdentifier
                  .Setup(i => i.GenerateContentId(file)).Returns("Foo");
                compiler.Setup(c => c.CanCompile(file)).Returns(true);
                compiler.Setup(c => c.Compile(file)).Returns(Task.FromResult(CompilationResult.Successful(It.IsAny<string>(), compiled.Object, new[] 
                {
                    new CompilationMessage(MessageLevel.Info, "Foo")
                })));

                // Act
                var result = await cm.Compile(file, NullTrace.Instance);

                // Assert
                Assert.True(result.Success);
                Assert.False(result.SatisfiedFromCache);
                Assert.Equal("Foo", result.Messages.Single().Message);
                Assert.Same(compiled.Object, result.GetCompiledType());

                Type cached;
                Assert.True(cm.Cache["Foo"].TryGetTarget(out cached));
                Assert.Same(compiled.Object, cached);
            }

            [Fact]
            public void ReturnsAndCachesCompiledResultIfCachedValueHasBeenCollected()
            {
                // Arrange
                var compiled = new Mock<Type>();
                var cm = CreateManager();
                var file = TestData.CreateDummyFile();
                var compiler = new Mock<ICompiler>();
                cm.Compilers.Add(compiler.Object);
                cm.MockContentIdentifier
                  .Setup(i => i.GenerateContentId(file)).Returns("Foo");
                compiler.Setup(c => c.CanCompile(file)).Returns(true);
                compiler.Setup(c => c.Compile(file)).Returns(Task.FromResult(CompilationResult.Successful(It.IsAny<string>(), compiled.Object, new[] 
                {
                    new CompilationMessage(MessageLevel.Info, "Foo")
                })));

                // Add a cache entry, but collect it.
                cm.Cache["Foo"] = new WeakReference<Type>(new FakeType());
                GC.Collect();

                // Act
                // Using ".Result" because making this Async causes a reference to the cached type to be held.
                var result = cm.Compile(file, NullTrace.Instance).Result;

                // Assert
                Assert.True(result.Success);
                Assert.False(result.SatisfiedFromCache);
                Assert.Equal("Foo", result.Messages.Single().Message);
                Assert.Same(compiled.Object, result.GetCompiledType());

                Type cached;
                Assert.True(cm.Cache["Foo"].TryGetTarget(out cached));
                Assert.Same(compiled.Object, cached);
            }

            [Fact]
            public async Task ReturnsFailedResultIfNoCompilersRegistered()
            {
                // Arrange
                var compiled = new Mock<Type>();
                var cm = CreateManager();
                var file = TestData.CreateDummyFile();
                cm.MockContentIdentifier
                  .Setup(i => i.GenerateContentId(file)).Returns("Foo");
                
                // Act
                var result = await cm.Compile(file, NullTrace.Instance);

                // Assert
                Assert.False(result.Success);
                Assert.Equal(
                    Resources.DefaultCompilationManager_CannotFindCompiler,
                    result.Messages.Single().Message);
            }

            [Fact]
            public async Task ReturnsFailedResultIfNoRegisteredCompilerCanCompileInput()
            {
                // Arrange
                var compiled = new Mock<Type>();
                var cm = CreateManager();
                var file = TestData.CreateDummyFile();
                var compiler = new Mock<ICompiler>(MockBehavior.Strict);
                cm.MockContentIdentifier
                  .Setup(i => i.GenerateContentId(file))
                  .Returns("Foo");
                compiler.Setup(c => c.CanCompile(file)).Returns(false);
                cm.Compilers.Add(compiler.Object);

                // Act
                var result = await cm.Compile(file, NullTrace.Instance);

                // Assert
                Assert.False(result.Success);
                Assert.Equal(
                    Resources.DefaultCompilationManager_CannotFindCompiler,
                    result.Messages.Single().Message);
            }
        }

        public class TestableDefaultCompilationManager : DefaultCompilationManager
        {
            public TestableDefaultCompilationManager()
            {
                ContentIdentifier = (MockContentIdentifier = new Mock<IContentIdentifier>()).Object;
                Compilers.Clear();
            }

            public Mock<IContentIdentifier> MockContentIdentifier { get; private set; }
        }
    }
}
