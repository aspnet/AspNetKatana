// -----------------------------------------------------------------------
// <copyright file="SharedOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    /// <summary>
    /// Options common to several middleware components
    /// </summary>
    public class SharedOptions
    {
        /// <summary>
        /// Defaults to all request paths and the current physical directory.
        /// </summary>
        public SharedOptions()
        {
            RequestPath = string.Empty;
            FileSystemProvider = new PhysicalFileSystemProvider(".");
        }

        /// <summary>
        /// The request path that maps to static resources
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// The file system used to locate resources
        /// </summary>
        public IFileSystemProvider FileSystemProvider { get; set; }
    }
}
