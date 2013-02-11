// <copyright file="TestFile.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.IO;
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
