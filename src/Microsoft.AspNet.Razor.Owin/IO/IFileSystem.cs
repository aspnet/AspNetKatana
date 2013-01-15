// -----------------------------------------------------------------------
// <copyright file="IFileSystem.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Owin.IO
{
    public interface IFileSystem
    {
        string Root { get; }
        IFile GetFile(string path);
    }
}
