// -----------------------------------------------------------------------
// <copyright file="TestFile.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Owin.IO;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    internal class TestFile : IFile
    {
        public TestFile(string fullPath, string path)
        {
            FullPath = fullPath;
            Path = path;
            Exists = false;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Extension = System.IO.Path.GetExtension(path);
            TextContent = String.Empty;
            LastModifiedTime = DateTime.UtcNow;
        }

        public TestFile(string fullPath, string path, string textContent) : this(fullPath, path)
        {
            TextContent = textContent;
            Exists = true;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }
        public string FullPath { get; private set; }
        public string Extension { get; private set; }
        public bool Exists { get; private set; }
        public string TextContent { get; private set; }
        public DateTime LastModifiedTime { get; private set; }
        
        public TextReader OpenRead()
        {
            return new StringReader(TextContent);
        }
    }
}
