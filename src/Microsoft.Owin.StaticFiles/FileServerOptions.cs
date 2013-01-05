// -----------------------------------------------------------------------
// <copyright file="FileServerOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    public class FileServerOptions
    {
        public FileServerOptions()
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

        public FileServerOptions WithRequestPath(string path)
        {
            RequestPath = path;
            return this;
        }

        public FileServerOptions WithFileSystemProvider(IFileSystemProvider fileSystemProvider)
        {
            FileSystemProvider = fileSystemProvider;
            return this;
        }

        public FileServerOptions WithPhysicalPath(string path)
        {
            return WithFileSystemProvider(new PhysicalFileSystemProvider(path));
        }

        public FileServerOptions WithContentTypeProvider(IContentTypeProvider contentTypeProvider)
        {
            ContentTypeProvider = contentTypeProvider;
            return this;
        }

        public FileServerOptions WithDefaultContentType(string path)
        {
            DefaultContentType = path;
            return this;
        }

        public FileServerOptions WithDefaultFiles(IEnumerable<string> defaultFiles)
        {
            DefaultFileNames = defaultFiles.ToList();
            return this;
        }

        internal StaticFileOptions GetStaticFileOptions()
        {
            StaticFileOptions staticFileOptions = new StaticFileOptions();
            staticFileOptions.RequestPath = RequestPath;
            staticFileOptions.FileSystemProvider = FileSystemProvider;
            staticFileOptions.ContentTypeProvider = ContentTypeProvider;
            staticFileOptions.DefaultContentType = DefaultContentType;
            return staticFileOptions;
        }

        internal DirectoryBrowserOptions GetDirectoryBrowserOptions()
        {
            DirectoryBrowserOptions directoryBrowserOptions = new DirectoryBrowserOptions();
            directoryBrowserOptions.RequestPath = RequestPath;
            directoryBrowserOptions.FileSystemProvider = FileSystemProvider;
            return directoryBrowserOptions;
        }

        internal DefaultFileOptions GetDefaultFileOptions()
        {
            DefaultFileOptions defaultFileOptions = new DefaultFileOptions();
            defaultFileOptions.RequestPath = RequestPath;
            defaultFileOptions.FileSystemProvider = FileSystemProvider;
            defaultFileOptions.WithDefaultFiles(DefaultFileNames);
            return defaultFileOptions;
        }
    }
}
