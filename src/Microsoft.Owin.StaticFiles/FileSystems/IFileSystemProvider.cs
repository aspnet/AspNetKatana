// -----------------------------------------------------------------------
// <copyright file="IFileSystemProvider.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    /// <summary>
    /// A file system abstraction
    /// </summary>
    public interface IFileSystemProvider
    {
        /// <summary>
        /// Locate a file at the given path, if any
        /// </summary>
        /// <param name="subpath">The path that identifies the file</param>
        /// <param name="fileInfo">The discovered file, if any</param>
        /// <returns>True if a file was located at the given path</returns>
        bool TryGetFileInfo(string subpath, out IFileInfo fileInfo);

        /// <summary>
        /// Locate a directory at the given path, if any
        /// </summary>
        /// <param name="subpath">The path that identifies the file</param>
        /// <param name="directoryInfo">The discovered directory, if any</param>
        /// <returns>True if a directory was located at the given path</returns>
        bool TryGetDirectoryInfo(string subpath, out IDirectoryInfo directoryInfo);
    }
}
