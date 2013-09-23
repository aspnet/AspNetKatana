// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.FileSystems.Tests
{
    public class EmbeddedResourceFileSystemTests
    {
        [Fact]
        public void When_TryGetFileInfo_and_resource_does_not_exist_then_should_not_get_file_info()
        {
            var provider = new EmbeddedResourceFileSystem("Microsoft.Owin.FileSystems.Tests.Resources");

            IFileInfo fileInfo;
            provider.TryGetFileInfo("DoesNotExist.Txt", out fileInfo).ShouldBe(false);

            fileInfo.ShouldBe(null);
        }

        [Fact]
        public void When_TryGetFileInfo_and_resource_exists_in_root_then_should_get_file_info()
        {
            var provider = new EmbeddedResourceFileSystem("Microsoft.Owin.FileSystems.Tests.Resources");

            IFileInfo fileInfo;
            provider.TryGetFileInfo("File.txt", out fileInfo).ShouldBe(true);

            fileInfo.ShouldNotBe(null);
            fileInfo.LastModified.ShouldNotBe(default(DateTime));
            fileInfo.Length.ShouldBeGreaterThan(0);
            fileInfo.IsDirectory.ShouldBe(false);
            fileInfo.PhysicalPath.ShouldBe(null);
            // TODO: fileInfo.Name.Should().Be(???)
        }

        [Fact]
        public void When_TryGetFileInfo_and_resources_in_path_then_should_get_file_infos()
        {
            var provider = new EmbeddedResourceFileSystem("Microsoft.Owin.FileSystems.Tests.Resources");

            IFileInfo fileInfo;
            provider.TryGetFileInfo("File.txt", out fileInfo).ShouldBe(true);

            fileInfo.ShouldNotBe(null);
            fileInfo.LastModified.ShouldNotBe(default(DateTime));
            fileInfo.Length.ShouldBeGreaterThan(0);
            fileInfo.IsDirectory.ShouldBe(false);
            fileInfo.PhysicalPath.ShouldBe(null);
            // TODO: fileInfo.Name.Should().Be(???)
        }
    }
}
