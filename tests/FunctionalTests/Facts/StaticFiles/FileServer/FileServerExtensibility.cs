// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public partial class FileServerExtensibility
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void Static_CustomFileSystem(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, CustomFileSystemConfiguration);

                HttpResponseMessage response;
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response);

                Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
                for (int index = 0; index < 10; index++)
                {
                    var fileSystemEntryName = (index % 2 != 0) ? string.Format("TextFile{0}.txt", index) : string.Format("Dir{0}/", index);
                    Assert.Contains(fileSystemEntryName, responseText);
                }
            }
        }

        internal void CustomFileSystemConfiguration(IAppBuilder app)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions() { FileSystem = new MyFileSystem() });
        }

        public class MyFileSystem : IFileSystem
        {
            public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
            {
                if (Path.GetExtension(subpath) == string.Empty)
                {
                    var contentsList = new List<IFileInfo>();
                    for (int i = 0; i < 10; i++)
                    {
                        contentsList.Add(new MyFileInfo(i));
                    }
                    contents = contentsList.ToArray();
                    return true;
                }

                contents = null;
                return false;
            }

            public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
            {
                if (Path.GetExtension(subpath) != string.Empty)
                {
                    fileInfo = new MyFileInfo(0);
                    return true;
                }

                fileInfo = null;
                return false;
            }
        }

        public class MyFileInfo : IFileInfo
        {
            private string name;

            public MyFileInfo(int index)
            {
                this.IsDirectory = (index % 2 == 0);
                this.name = !this.IsDirectory ? string.Format("TextFile{0}.txt", index) : string.Format("Dir{0}", index);
            }

            public System.IO.Stream CreateReadStream()
            {
                throw new NotImplementedException();
            }

            public bool IsDirectory { get; private set; }

            public DateTime LastModified
            {
                get { return DateTime.Now; }
            }

            public long Length { get { return new Random().Next(1, 5000); } }

            public string Name { get { return this.name; } }

            public string PhysicalPath { get { return string.Format(".\\{0}", this.name); } }
        }
    }
}