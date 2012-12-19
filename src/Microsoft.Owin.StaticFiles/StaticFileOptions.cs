// -----------------------------------------------------------------------
// <copyright file="StaticFileOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    public class StaticFileOptions
    {
        public StaticFileOptions()
        {
            RequestPath = string.Empty;
            FileSystemProvider = new PhysicalFileSystemProvider(".");
            ContentTypeProvider = new DefaultContentTypeProvider();
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
        public IContentTypeProvider ContentTypeProvider { get; set; }
        public string DefaultContentType { get; set; }
        public IList<string> DefaultFileNames { get; private set; }

        public StaticFileOptions WithRequestPath(string path)
        {
            RequestPath = path;
            return this;
        }

        public StaticFileOptions WithFileSystemProvider(IFileSystemProvider fileSystemProvider)
        {
            FileSystemProvider = fileSystemProvider;
            return this;
        }

        public StaticFileOptions WithPhysicalPath(string path)
        {
            return WithFileSystemProvider(new PhysicalFileSystemProvider(path));
        }

        public StaticFileOptions WithContentTypeProvider(IContentTypeProvider contentTypeProvider)
        {
            ContentTypeProvider = contentTypeProvider;
            return this;
        }

        public StaticFileOptions WithDefaultContentType(string path)
        {
            DefaultContentType = path;
            return this;
        }

        public StaticFileOptions WithDefaultFiles(IEnumerable<string> defaultFiles)
        {
            DefaultFileNames = defaultFiles.ToList();
            return this;
        }
    }
}
