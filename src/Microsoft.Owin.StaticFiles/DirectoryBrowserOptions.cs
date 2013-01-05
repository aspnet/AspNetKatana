// -----------------------------------------------------------------------
// <copyright file="DirectoryBrowserOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    public class DirectoryBrowserOptions
    {
        public DirectoryBrowserOptions()
        {
            RequestPath = string.Empty;
            FileSystemProvider = new PhysicalFileSystemProvider(".");
        }

        public string RequestPath { get; set; }
        public IFileSystemProvider FileSystemProvider { get; set; }

        public DirectoryBrowserOptions WithRequestPath(string path)
        {
            RequestPath = path;
            return this;
        }

        public DirectoryBrowserOptions WithFileSystemProvider(IFileSystemProvider fileSystemProvider)
        {
            FileSystemProvider = fileSystemProvider;
            return this;
        }

        public DirectoryBrowserOptions WithPhysicalPath(string path)
        {
            return WithFileSystemProvider(new PhysicalFileSystemProvider(path));
        }
    }
}
