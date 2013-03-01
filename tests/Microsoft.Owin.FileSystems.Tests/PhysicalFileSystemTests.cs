// <copyright file="PhysicalFileSystemTests.cs" company="Microsoft Open Technologies, Inc.">
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
    }
}
