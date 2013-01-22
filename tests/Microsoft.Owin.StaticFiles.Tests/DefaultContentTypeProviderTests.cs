// <copyright file="DefaultContentTypeProviderTests.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using Microsoft.Owin.StaticFiles.ContentTypes;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class DefaultContentTypeProviderTests
    {
        [Fact]
        public void UnknownExtensionsReturnFalse()
        {
            var provider = new DefaultContentTypeProvider();
            string contentType;
            provider.TryGetContentType("unknown.ext", out contentType).ShouldBe(false);
        }

        [Fact]
        public void KnownExtensionsReturnTrye()
        {
            var provider = new DefaultContentTypeProvider();
            string contentType;
            provider.TryGetContentType("known.txt", out contentType).ShouldBe(true);
            contentType.ShouldBe("text/plain");
        }

        [Fact]
        public void DoubleDottedExtensionsAreNotSupported()
        {
            var provider = new DefaultContentTypeProvider();
            string contentType;
            provider.TryGetContentType("known.exe.config", out contentType).ShouldBe(false);
        }

        [Fact]
        public void DashedExtensionsShouldBeMatched()
        {
            var provider = new DefaultContentTypeProvider();
            string contentType;
            provider.TryGetContentType("known.dvr-ms", out contentType).ShouldBe(true);
            contentType.ShouldBe("video/x-ms-dvr");
        }

        [Fact]
        public void BothSlashFormatsAreUnderstood()
        {
            var provider = new DefaultContentTypeProvider();
            string contentType;
            provider.TryGetContentType(@"/first/example.txt", out contentType).ShouldBe(true);
            contentType.ShouldBe("text/plain");
            provider.TryGetContentType(@"\second\example.txt", out contentType).ShouldBe(true);
            contentType.ShouldBe("text/plain");
        }

        [Fact]
        public void DotsInDirectoryAreIgnored()
        {
            var provider = new DefaultContentTypeProvider();
            string contentType;
            provider.TryGetContentType(@"/first.css/example.txt", out contentType).ShouldBe(true);
            contentType.ShouldBe("text/plain");
            provider.TryGetContentType(@"\second.css\example.txt", out contentType).ShouldBe(true);
            contentType.ShouldBe("text/plain");
        }
    }
}
