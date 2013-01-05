// -----------------------------------------------------------------------
// <copyright file="DefaultFileOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    public class DefaultFileOptions
    {
        public DefaultFileOptions()
        {
            RequestPath = string.Empty;
            FileSystemProvider = new PhysicalFileSystemProvider(".");

            // Prioritized list
            DefaultFileNames = new List<string>()
            {
                "default.htm",
                "default.html",
                "index.htm",
                "index.html",
                "default.aspx",
            };
        }

        public string RequestPath { get; set; }
        public IFileSystemProvider FileSystemProvider { get; set; }

        public IList<string> DefaultFileNames { get; private set; }

        public DefaultFileOptions WithRequestPath(string path)
        {
            RequestPath = path;
            return this;
        }

        public DefaultFileOptions WithFileSystemProvider(IFileSystemProvider fileSystemProvider)
        {
            FileSystemProvider = fileSystemProvider;
            return this;
        }

        public DefaultFileOptions WithPhysicalPath(string path)
        {
            return WithFileSystemProvider(new PhysicalFileSystemProvider(path));
        }

        public DefaultFileOptions WithDefaultFiles(IEnumerable<string> defaultFiles)
        {
            DefaultFileNames = defaultFiles.ToList();
            return this;
        }
    }
}
