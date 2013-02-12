// <copyright file="DefaultCompilationManagerTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Threading.Tasks;
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
                IContentIdentifier contentIder = new Mock<IContentIdentifier>().Object;

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
                ContractAssert.NotNull(() => CreateManager().Compile(new TestFile("a", "file", "text"), null), "tracer");
            }

            [Fact]
            public async Task ReturnsCacheIfHitAndReferenceValid()
            {
                // Arrange
                var cached = new Mock<Type>();
                TestableDefaultCompilationManager cm = CreateManager();
                TestFile file = TestData.CreateDummyFile();
                cm.Cache["Foo"] = new WeakReference<Type>(cached.Object);
                cm.MockContentIdentifier
                    .Setup(i => i.GenerateContentId(file)).Returns("Foo");

                // Act
                CompilationResult result = await cm.Compile(file, NullTrace.Instance);

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
                TestableDefaultCompilationManager cm = CreateManager();
                TestFile file = TestData.CreateDummyFile();
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
                CompilationResult result = await cm.Compile(file, NullTrace.Instance);

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
                TestableDefaultCompilationManager cm = CreateManager();
                TestFile file = TestData.CreateDummyFile();
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
                CompilationResult result = cm.Compile(file, NullTrace.Instance).Result;

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
                TestableDefaultCompilationManager cm = CreateManager();
                TestFile file = TestData.CreateDummyFile();
                cm.MockContentIdentifier
                    .Setup(i => i.GenerateContentId(file)).Returns("Foo");

                // Act
                CompilationResult result = await cm.Compile(file, NullTrace.Instance);

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
                TestableDefaultCompilationManager cm = CreateManager();
                TestFile file = TestData.CreateDummyFile();
                var compiler = new Mock<ICompiler>(MockBehavior.Strict);
                cm.MockContentIdentifier
                    .Setup(i => i.GenerateContentId(file))
                    .Returns("Foo");
                compiler.Setup(c => c.CanCompile(file)).Returns(false);
                cm.Compilers.Add(compiler.Object);

                // Act
                CompilationResult result = await cm.Compile(file, NullTrace.Instance);

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
