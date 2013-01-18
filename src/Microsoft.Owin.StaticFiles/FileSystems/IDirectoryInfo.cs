// -----------------------------------------------------------------------
// <copyright file="IDirectoryInfo.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    /// <summary>
    /// Represents a directory of files and directories.
    /// </summary>
    public interface IDirectoryInfo
    {
        /// <summary>
        /// The path to this directory, including its name
        /// </summary>
        string PhysicalPath { get; }

        /// <summary>
        /// The name of this directory
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Any sub directories in the current directory</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        IEnumerable<IDirectoryInfo> GetDirectories();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Any files in the current directory</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        IEnumerable<IFileInfo> GetFiles();
    }
}
