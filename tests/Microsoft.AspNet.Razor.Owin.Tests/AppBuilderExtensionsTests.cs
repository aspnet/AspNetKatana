// -----------------------------------------------------------------------
// <copyright file="AppBuilderExtensionsTests.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Owin;
using Microsoft.AspNet.Razor.Owin.Compilation;
using Microsoft.AspNet.Razor.Owin.Execution;
using Microsoft.AspNet.Razor.Owin.IO;
using Microsoft.AspNet.Razor.Owin.Routing;
using Owin;
using Owin.Builder;
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
            RazorApplication app = del.Target as RazorApplication;
            Assert.NotNull(app);
            Assert.Equal(virtualPath, app.VirtualRoot);
            if (expectedFs is PhysicalFileSystem)
            {
                Assert.Equal(
                    expectedFs.Root,
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
