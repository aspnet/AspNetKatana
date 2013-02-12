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
using System.Text;
using Microsoft.Owin.FileSystems;

namespace Microsoft.AspNet.Razor.Owin.Tests
{
    internal class TestFile : IFileInfo
    {
        //public TestFile(string fullPath, string path)
        //{
        //}

        public TestFile(string fullPath, string path, string textContent)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
            {
                writer.Write(TextContent);
            }
            Length = memoryStream.Length;
            PhysicalPath = fullPath;
            Name = Path.GetFileName(fullPath);
            LastModified = DateTime.UtcNow;
            TextContent = textContent;
        }

        public string TextContent { get; private set; }

        public long Length { get; private set; }
        public string PhysicalPath { get; private set; }
        public string Name { get; private set; }
        public DateTime LastModified { get; private set; }

        public Stream CreateReadStream()
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
            {
                writer.Write(TextContent);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
