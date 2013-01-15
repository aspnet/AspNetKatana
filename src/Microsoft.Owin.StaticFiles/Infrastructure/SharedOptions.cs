// -----------------------------------------------------------------------
// <copyright file="SharedOptions.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    public class SharedOptions
    {
        public SharedOptions()
        {
            RequestPath = string.Empty;
            FileSystemProvider = new PhysicalFileSystemProvider(".");
        }

        public string RequestPath { get; set; }

        public IFileSystemProvider FileSystemProvider { get; set; }
    }
}
