// <copyright file="PhysicalFileSystemProviderTests.cs" company="Microsoft Open Technologies, Inc.">
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

using Microsoft.Owin.StaticFiles.FileSystems;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class PhysicalFileSystemProviderTests
    {
        [Fact]
        public void ExistingFilesReturnTrue()
        {
            var provider = new PhysicalFileSystemProvider(".");
            IFileInfo info;
            provider.TryGetFileInfo("/Owin.dll", out info).ShouldBe(true);
            info.ShouldNotBe(null);
        }

        [Fact]
        public void MissingFilesReturnFalse()
        {
            var provider = new PhysicalFileSystemProvider(".");
            IFileInfo info;
            provider.TryGetFileInfo("/Owin5.dll", out info).ShouldBe(false);
            info.ShouldBe(null);
        }
    }
}
