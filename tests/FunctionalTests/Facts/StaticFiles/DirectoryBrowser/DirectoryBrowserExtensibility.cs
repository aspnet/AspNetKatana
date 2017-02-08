// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.DirectoryFormatters;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class DirectoryBrowserExtensibility
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_DirectoryBrowserCustomFormatter(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, DirectoryBrowserCustomFormatterConfiguration);
                HttpResponseMessage response;
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response);

                Assert.Equal<string>("custom/format", response.Content.Headers.ContentType.MediaType);

                var lines = responseText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                Assert.NotEqual<int>(0, lines.Length);
                lines.All(line =>
                {
                    Trace.WriteLine(line);
                    var fileProperties = line.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    Assert.True(fileProperties.Count() == 5, "Insufficient file details. There should be five parts");
                    return true;
                });
            }
        }

        public void DirectoryBrowserCustomFormatterConfiguration(IAppBuilder app)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions() { Formatter = new MyDirectoryInfoFormatter() });
        }
    }
    public class MyDirectoryInfoFormatter : IDirectoryFormatter
    {
        public Task GenerateContentAsync(IOwinContext context, IEnumerable<IFileInfo> contents)
        {
            var directoryContent = string.Join("\n",
                            contents.Select<IFileInfo, string>(content =>
                            {
                                return string.Format("{0}#{1}#{2}#{3}#{4}",
                                    content.Name, content.IsDirectory, content.LastModified, content.Length, content.PhysicalPath);
                            }));

            context.Response.ContentType = "custom/format";
            context.Response.WriteAsync("MyDirectoryInfoFormatter");
            return context.Response.WriteAsync(string.Join("\n", directoryContent));
        }
    }
}
