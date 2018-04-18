﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.FileSystems.Tests
{
    public class PhysicalFileSystemTests
    {
        [Fact]
        public void ExistingFilesReturnTrue()
        {
            var provider = new PhysicalFileSystem(".");
            IFileInfo info;
            provider.TryGetFileInfo("File.txt", out info).ShouldBe(true);
            info.ShouldNotBe(null);
        }

        [Fact]
        public void MissingFilesReturnFalse()
        {
            var provider = new PhysicalFileSystem(".");
            IFileInfo info;
            provider.TryGetFileInfo("File5.txt", out info).ShouldBe(false);
            info.ShouldBe(null);
        }

        [Fact]
        public void SubPathActsAsRoot()
        {
            var provider = new PhysicalFileSystem("sub");
            IFileInfo info;
            provider.TryGetFileInfo("File2.txt", out info).ShouldBe(true);
            info.ShouldNotBe(null);
        }

        [Fact]
        public void RelativeOrAbsolutePastRootNotAllowed()
        {
            var provider = new PhysicalFileSystem("sub");
            IFileInfo info;
            
            provider.TryGetFileInfo("..\\File.txt", out info).ShouldBe(false);
            info.ShouldBe(null);
            
            provider.TryGetFileInfo(".\\..\\File.txt", out info).ShouldBe(false);
            info.ShouldBe(null);

            var applicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var file1 = Path.Combine(applicationBase, "File.txt");
            var file2 = Path.Combine(applicationBase, "sub", "File2.txt");
            provider.TryGetFileInfo(file1, out info).ShouldBe(false);
            info.ShouldBe(null);

            provider.TryGetFileInfo(file2, out info).ShouldBe(true);
            info.ShouldNotBe(null);
            info.PhysicalPath.ShouldBe(file2);
        }

        [Fact]
        public void NotSupportedCharactersInPathReturnFalse()
        {
            var provider = new PhysicalFileSystem("sub");
            IFileInfo info;
            provider.TryGetFileInfo("(DefaultRouterOutlet:type)", out info).ShouldBe(false);
            info.ShouldBe(null);
        }
    }
}
