// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    /// <summary>
    /// Generates the view for a directory
    /// </summary>
    public interface IDirectoryInfoFormatter
    {
        /// <summary>
        /// Generates the view for a directory
        /// </summary>
        Task GenerateContentAsync(IOwinContext context, IEnumerable<IFileInfo> contents);
    }
}
