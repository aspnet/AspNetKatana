// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    /// <summary>
    /// Generates the view for a directory
    /// </summary>
    public interface IDirectoryFormatter
    {
        /// <summary>
        /// Generates the view for a directory.
        /// Implementers should properly handle HEAD requests.
        /// Implementers should set all necessary response headers (e.g. Content-Type, Content-Length, etc.).
        /// </summary>
        Task GenerateContentAsync(IOwinContext context, IEnumerable<IFileInfo> contents);
    }
}
