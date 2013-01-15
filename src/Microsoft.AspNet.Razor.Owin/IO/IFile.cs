// -----------------------------------------------------------------------
// <copyright file="IFile.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Razor.Owin.IO
{
    public interface IFile
    {
        string Name { get; }
        string Path { get; }
        string FullPath { get; }
        string Extension { get; }
        bool Exists { get; }
        DateTime LastModifiedTime { get; }
        TextReader OpenRead();
    }
}
