// -----------------------------------------------------------------------
// <copyright file="PhysicalFileSystem.cs" company="Microsoft">
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
    public class PhysicalFileSystem : IFileSystem
    {
        public PhysicalFileSystem(string root)
        {
            Root = root;
        }

        public string Root { get; private set; }

        public IFile GetFile(string path)
        {
            return new PhysicalFile(Root, path);
        }
    }
}
