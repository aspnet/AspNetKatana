// -----------------------------------------------------------------------
// <copyright file="IFileInfo.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    public interface IFileInfo
    {
        long Length { get; }
        string PhysicalPath { get; }
        string Name { get; }
        DateTime LastModified { get; }
    }
}