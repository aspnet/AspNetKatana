// <copyright file="PhysicalFile.cs" company="Microsoft Open Technologies, Inc.">
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
