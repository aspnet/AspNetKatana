// -----------------------------------------------------------------------
// <copyright file="FileServerOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.DirectoryFormatters;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Microsoft.Owin.StaticFiles
{
    public class FileServerOptions : SharedOptionsBase<FileServerOptions>
    {
        public FileServerOptions()
            : base(new SharedOptions())
        {
            StaticFileOptions = new StaticFileOptions(_sharedOptions);
            DirectoryBrowserOptions = new DirectoryBrowserOptions(_sharedOptions);
            DefaultFilesOptions = new DefaultFilesOptions(_sharedOptions);
        }

        public StaticFileOptions StaticFileOptions { get; set; }
        public DirectoryBrowserOptions DirectoryBrowserOptions { get; set; }
        public DefaultFilesOptions DefaultFilesOptions { get; set; }

        public FileServerOptions WithContentTypeProvider(IContentTypeProvider contentTypeProvider)
        {
            StaticFileOptions.WithContentTypeProvider(contentTypeProvider);
            return this;
        }

        public FileServerOptions WithDefaultContentType(string defaultContentType)
        {
            StaticFileOptions.WithDefaultContentType(defaultContentType);
            return this;
        }

        public FileServerOptions WithDefaultFileNames(IEnumerable<string> defaultFileNames)
        {
            DefaultFilesOptions.WithDefaultFileNames(defaultFileNames);
            return this;
        }

        public FileServerOptions WithFormatSelector(IDirectoryFormatSelector formatSelector)
        {
            DirectoryBrowserOptions.WithFormatSelector(formatSelector);
            return this;
        }
    }
}
