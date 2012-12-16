using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
