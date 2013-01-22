// <copyright file="TestFileSystem.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Razor.Owin.IO;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    internal class TestFileSystem : IFileSystem
    {
        private readonly Dictionary<string, IFile> _testFiles = new Dictionary<string, IFile>(StringComparer.OrdinalIgnoreCase);

        public TestFileSystem(string root)
        {
            Root = root;
        }

        public string Root { get; private set; }

        public IFile GetFile(string path)
        {
            IFile file;
            if (!_testFiles.TryGetValue(path, out file))
            {
                return new TestFile(Path.Combine(Root, path), path);
            }
            return file;
        }

        public IFile AddTestFile(string path)
        {
            return AddTestFile(path, "Content is irrelevant!");
        }

        public IFile AddTestFile(string path, string content)
        {
            var file = new TestFile(Path.Combine(Root, path), path, content);
            _testFiles.Add(path, file);
            return file;
        }
    }
}
