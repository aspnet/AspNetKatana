// -----------------------------------------------------------------------
// <copyright file="IFileSystemProvider.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.IO;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    public interface IFileSystemProvider
    {
        bool TryGetFileInfo(string subpath, out IFileInfo fileInfo);

        bool TryGetDirectoryInfo(string subpath, out IDirectoryInfo directoryInfo);
    }
}
