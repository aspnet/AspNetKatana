// <copyright file="TestFileSystem.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.IO;
using Microsoft.Owin.FileSystems;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    internal class TestFileSystem : IFileSystem
    {
        private readonly Dictionary<string, IFileInfo> _testFiles = new Dictionary<string, IFileInfo>(StringComparer.OrdinalIgnoreCase);

        public TestFileSystem(string root)
        {
            Root = root;
        }

        public string Root { get; private set; }


        public IFileInfo AddTestFile(string path)
        {
            return AddTestFile(path, "Content is irrelevant!");
        }

        public IFileInfo AddTestFile(string path, string content)
        {
            var file = new TestFile(Path.Combine(Root, path), path, content);
            _testFiles.Add(path, file);
            return file;
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            return _testFiles.TryGetValue(subpath, out fileInfo);
        }

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> directoryInfo)
        {
            throw new NotImplementedException();
        }
    }
}
