// <copyright file="PhysicalFileSystem.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.FileSystems
{
    /// <summary>
    /// Looks up files using the on-disk file system
    /// </summary>
    public class PhysicalFileSystem : IFileSystem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="root">The root directory</param>
        public PhysicalFileSystem(string root)
        {
            Root = root;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="fileInfo">The discovered file, if any</param>
        /// <returns>True if a file was discovered at the given path</returns>
        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            try
            {
                var info = new FileInfo(Combine(Root, subpath));
                if (info.Exists)
                {
                    fileInfo = new PhysicalFileInfo(info);
                    return true;
                }
            }
            catch (ArgumentException)
            {
            }
            fileInfo = null;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <param name="contents">The discovered directories, if any</param>
        /// <returns>True if a directory was discovered at the given path</returns>
        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(Combine(Root, subpath));
                
                FileSystemInfo[] physicalInfos = directoryInfo.GetFileSystemInfos();
                var virtualInfos = new IFileInfo[physicalInfos.Length];
                for (int index = 0; index != physicalInfos.Length; ++index)
                {
                    var fileInfo = physicalInfos[index] as FileInfo;
                    if (fileInfo != null)
                    {
                        virtualInfos[index] = new PhysicalFileInfo(fileInfo);
                    }
                    else
                    {
                        virtualInfos[index] = new PhysicalDirectoryInfo((DirectoryInfo)physicalInfos[index]);
                    }
                }
                contents = virtualInfos;
                return true;
            }
            catch (ArgumentException)
            {
            }
            contents = null;
            return false;
        }

        private static string Combine(string path1, string path2)
        {
            if (string.IsNullOrWhiteSpace(path1))
            {
                return path2;
            }

            if (string.IsNullOrWhiteSpace(path2))
            {
                return path1;
            }

            // path1, path2
            if (!path1.EndsWith("/", StringComparison.Ordinal)
                && !path1.EndsWith(@"\", StringComparison.Ordinal)
                && !path2.StartsWith("/", StringComparison.Ordinal)
                && !path2.StartsWith(@"\", StringComparison.Ordinal))
            {
                return path1 + "/" + path2;
            }
            // path1/, /path2
            if ((path1.EndsWith("/", StringComparison.Ordinal)
                || path1.EndsWith(@"\", StringComparison.Ordinal))
                && (path2.StartsWith("/", StringComparison.Ordinal)
                    || path2.StartsWith(@"\", StringComparison.Ordinal)))
            {
                return path1 + path2.Substring(1);
            }
            // path1, /path2 or path1/, path2
            return path1 + path2;
        }

        private class PhysicalFileInfo : IFileInfo
        {
            private readonly FileInfo _info;

            public PhysicalFileInfo(FileInfo info)
            {
                _info = info;
            }

            public long Length
            {
                get { return _info.Length; }
            }

            public string PhysicalPath
            {
                get { return _info.FullName; }
            }

            public string Name
            {
                get { return _info.Name; }
            }

            public DateTime LastModified
            {
                get { return _info.LastWriteTime; }
            }

            public Stream CreateReadStream()
            {
                return _info.OpenRead();
            }
        }

        private class PhysicalDirectoryInfo : IFileInfo
        {
            private readonly DirectoryInfo _info;

            public PhysicalDirectoryInfo(DirectoryInfo info)
            {
                _info = info;
            }

            public long Length { get { return -1; } }

            public string PhysicalPath
            {
                get { return _info.FullName; }
            }

            public string Name
            {
                get { return _info.Name; }
            }

            public DateTime LastModified
            {
                get { return _info.LastWriteTime; }
            }

            public Stream CreateReadStream()
            {
                return null;
            }
        }
    }
}
