// <copyright file="AppBuilderExtensionsTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using Owin;
using Xunit;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class AppBuilderExtensionsTests
    {
        private static readonly MethodInfo TheStartMethod = typeof(RazorApplication).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, new[] { typeof(AppFunc) }, new ParameterModifier[0]);

        public static void AssertEdgeApplication(Delegate del)
        {
            AssertEdgeApplication(del, "/");
        }

        public static void AssertEdgeApplication(Delegate del, string virtualPath)
        {
            AssertEdgeApplication(del, "/", new PhysicalFileSystem(Environment.CurrentDirectory));
        }

        public static void AssertEdgeApplication(Delegate del, string virtualPath, IFileSystem expectedFs)
        {
            var app = del.Target as RazorApplication;
            Assert.NotNull(app);
            Assert.Equal(virtualPath, app.VirtualRoot);
            if (expectedFs is PhysicalFileSystem)
            {
                Assert.Equal(
                    Assert.IsType<PhysicalFileSystem>(expectedFs).Root,
                    Assert.IsType<PhysicalFileSystem>(app.FileSystem).Root);
            }
            else
            {
                Assert.Equal(expectedFs, app.FileSystem);
            }
            Assert.Equal(del.Method, TheStartMethod);
        }

        public class TheUseEdgeMethod
        {
            public class WithNoArguments
            {
                [Fact]
                public void RequiresNonNullBuilder()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(null, "Foo"), "builder");
                }
            }

            public class WithRootDirectoryArgument
            {
                [Fact]
                public void RequiresNonNullBuilder()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(null, "Foo"), "builder");
                }

                [Fact]
                public void RequiresNonNullOrEmptyRootDirectory()
                {
                    ContractAssert.NotNullOrEmpty(s => RazorExtensions.UseRazor(new TestAppBuilder(), s), "rootDirectory");
                }
            }

            public class WithRootDirectoryAndVirtualRootArguments
            {
                [Fact]
                public void RequiresNonNullBuilder()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(null, "Foo", "/Bar"), "builder");
                }

                [Fact]
                public void RequiresNonNullOrEmptyRootDirectory()
                {
                    ContractAssert.NotNullOrEmpty(s => RazorExtensions.UseRazor(new TestAppBuilder(), s, "/Bar"), "rootDirectory");
                }

                [Fact]
                public void RequiresNonNullOrEmptyVirtualRoot()
                {
                    ContractAssert.NotNullOrEmpty(s => RazorExtensions.UseRazor(new TestAppBuilder(), "Foo", s), "virtualRoot");
                }
            }

            public class WithFileSystemArgument
            {
                [Fact]
                public void RequiresNonNullBuilder()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(null, new PhysicalFileSystem(@"C:\")), "builder");
                }

                [Fact]
                public void RequiresNonNullOrEmptyFileSystem()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(new TestAppBuilder(), (IFileSystem)null), "fileSystem");
                }
            }

            public class WithFileSystemAndVirtualRootArguments
            {
                [Fact]
                public void RequiresNonNullBuilder()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(null, new PhysicalFileSystem(@"C:\")), "builder");
                }

                [Fact]
                public void RequiresNonNullOrEmptyFileSystem()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(new TestAppBuilder(), (IFileSystem)null), "fileSystem");
                }

                [Fact]
                public void RequiresNonNullOrEmptyVirtualRoot()
                {
                    ContractAssert.NotNullOrEmpty(s => RazorExtensions.UseRazor(new TestAppBuilder(), new PhysicalFileSystem(@"C:\"), s), "virtualRoot");
                }
            }

            public class WithEdgeApplicationArgument
            {
                [Fact]
                public void RequiresNonNullBuilder()
                {
                    ContractAssert.NotNull(() => RazorExtensions.UseRazor(null, string.Empty), "builder");
                }
            }
        }
    }
}
