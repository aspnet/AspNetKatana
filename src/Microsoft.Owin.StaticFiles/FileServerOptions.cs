// <copyright file="FileServerOptions.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

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
        /// Directory browsing is disabled by default.
        /// </summary>
        public bool EnableDirectoryBrowsing { get; set; }

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
        /// <returns>this</returns>
        public FileServerOptions WithFormatSelector(IDirectoryFormatSelector formatSelector)
        {
            DirectoryBrowserOptions.WithFormatSelector(formatSelector);
            return this;
        }

        /// <summary>
        /// Enables directory browsing.
        /// </summary>
        /// <returns>this</returns>
        public FileServerOptions WithDirectoryBrowsing()
        {
            EnableDirectoryBrowsing = true;
            return this;
        }
    }
}
