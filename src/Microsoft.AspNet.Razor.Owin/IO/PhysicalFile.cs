// -----------------------------------------------------------------------
// <copyright file="PhysicalFile.cs" company="Microsoft">
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
    public class PhysicalFile : IFile
    {
        private readonly FileInfo _fileInfo;

        public PhysicalFile(string root, string relativePath)
        {
            Path = relativePath.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
            FullPath = System.IO.Path.Combine(root, Path);
            _fileInfo = new FileInfo(FullPath);
        }

        public string Path { get; private set; }
        public string FullPath { get; private set; }

        public bool Exists
        {
            get { return _fileInfo.Exists; }
        }

        public string Extension
        {
            get { return _fileInfo.Extension; }
        }

        public string Name
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(_fileInfo.Name); }
        }

        public DateTime LastModifiedTime
        {
            get { return _fileInfo.LastWriteTime.ToUniversalTime(); }
        }

        public TextReader OpenRead()
        {
            return _fileInfo.OpenText();
        }
    }
}
