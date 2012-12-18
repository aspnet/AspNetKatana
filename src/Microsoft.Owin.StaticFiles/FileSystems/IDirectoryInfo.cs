// -----------------------------------------------------------------------
// <copyright file="IDirectoryInfo.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    public interface IDirectoryInfo
    {
        string PhysicalPath { get; }
        string Name { get; }
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        IEnumerable<IDirectoryInfo> GetDirectories();
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        IEnumerable<IFileInfo> GetFiles();
    }
}
