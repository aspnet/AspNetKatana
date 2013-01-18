// -----------------------------------------------------------------------
// <copyright file="IFileInfo.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    /// <summary>
    /// Represents a file in the given file system.
    /// </summary>
    public interface IFileInfo
    {
        /// <summary>
        /// The length of the file in bytes
        /// </summary>
        long Length { get; }

        /// <summary>
        /// The path to the file, including the file name
        /// </summary>
        string PhysicalPath { get; }

        /// <summary>
        /// The name of the file
        /// </summary>
        string Name { get; }

        /// <summary>
        /// When the file was last modified
        /// </summary>
        DateTime LastModified { get; }
    }
}