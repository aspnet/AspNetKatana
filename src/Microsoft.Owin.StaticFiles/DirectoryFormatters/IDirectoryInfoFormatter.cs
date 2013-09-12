// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    /// <summary>
    /// Generates the view for a directory, depending on a specific content type
    /// </summary>
    public interface IDirectoryInfoFormatter
    {
        /// <summary>
        /// The content-type that describes the output generated
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Generates a view for the given directory
        /// </summary>
        /// <param name="requestPath">The request path</param>
        /// <param name="contents">The directory contents to render</param>
        /// <returns>The view, as a StringBuilder</returns>
        StringBuilder GenerateContent(PathString requestPath, IEnumerable<IFileInfo> contents);
    }
}
