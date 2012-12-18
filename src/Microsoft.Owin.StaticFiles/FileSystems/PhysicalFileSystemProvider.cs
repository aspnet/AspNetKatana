// -----------------------------------------------------------------------
// <copyright file="PhysicalFileSystemProvider.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
namespace Microsoft.Owin.StaticFiles.FileSystems
{
    public class PhysicalFileSystemProvider : IFileSystemProvider
    {
        private readonly string _path;

        public PhysicalFileSystemProvider(string path)
        {
            _path = path;
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            var info = new FileInfo(_path + subpath);
            if (info.Exists)
            {
                fileInfo = new PhysicalFileInfo(info);
                return true;
            }
            fileInfo = null;
            return false;
        }

        public bool TryGetDirectoryInfo(string subpath, out IDirectoryInfo directoryInfo)
        {
            var info = new DirectoryInfo(_path + subpath);
            if (info.Exists)
            {
                directoryInfo = new PhysicalDirectoryInfo(info);
                return true;
            }
            directoryInfo = null;
            return false;
        }

        internal class PhysicalFileInfo : IFileInfo
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
        }

        internal class PhysicalDirectoryInfo : IDirectoryInfo
        {
            private readonly DirectoryInfo _info;

            public PhysicalDirectoryInfo(DirectoryInfo info)
            {
                _info = info;
            }

            public string PhysicalPath
            {
                get { return _info.FullName; }
            }

            public string Name
            {
                get { return _info.Name;  }
            }

            public IEnumerable<IDirectoryInfo> GetDirectories()
            {
                foreach (DirectoryInfo dir in _info.GetDirectories())
                {
                    yield return new PhysicalDirectoryInfo(dir);
                }
            }

            public IEnumerable<IFileInfo> GetFiles()
            {
                foreach (FileInfo file in _info.GetFiles())
                {
                    yield return new PhysicalFileInfo(file);
                }
            }
        }
    }
}