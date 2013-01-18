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
    /// <summary>
    /// Options for all of the static file middleware components
    /// </summary>
    public class FileServerOptions : SharedOptionsBase<FileServerOptions>
    {
        /// <summary>
        /// 
        /// </summary>
        public FileServerOptions()
            : base(new SharedOptions())
        {
            StaticFileOptions = new StaticFileOptions(SharedOptions);
            DirectoryBrowserOptions = new DirectoryBrowserOptions(SharedOptions);
            DefaultFilesOptions = new DefaultFilesOptions(SharedOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        public StaticFileOptions StaticFileOptions { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DirectoryBrowserOptions DirectoryBrowserOptions { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DefaultFilesOptions DefaultFilesOptions { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentTypeProvider"></param>
        /// <returns>this</returns>
        public FileServerOptions WithContentTypeProvider(IContentTypeProvider contentTypeProvider)
        {
            StaticFileOptions.WithContentTypeProvider(contentTypeProvider);
            return this;
        }

        /// <summary>
        /// Used to look up MIME types given a file path
        /// </summary>
        /// <param name="defaultContentType"></param>
        /// <returns>this</returns>
        public FileServerOptions WithDefaultContentType(string defaultContentType)
        {
            StaticFileOptions.WithDefaultContentType(defaultContentType);
            return this;
        }

        /// <summary>
        /// Specifies the file names to serve by default
        /// </summary>
        /// <param name="defaultFileNames"></param>
        /// <returns>this</returns>
        public FileServerOptions WithDefaultFileNames(IEnumerable<string> defaultFileNames)
        {
            DefaultFilesOptions.WithDefaultFileNames(defaultFileNames);
            return this;
        }

        /// <summary>
        /// Specifies component that examines a request and selects a directory view formatter.
        /// </summary>
        /// <param name="formatSelector"></param>
        /// <returns></returns>
        public FileServerOptions WithFormatSelector(IDirectoryFormatSelector formatSelector)
        {
            DirectoryBrowserOptions.WithFormatSelector(formatSelector);
            return this;
        }
    }
}
