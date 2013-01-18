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
    /// <summary>
    /// Looks up files using the on-disk file system
    /// </summary>
    public class PhysicalFileSystemProvider : IFileSystemProvider
    {
        private readonly string _path;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The root directory</param>
        public PhysicalFileSystemProvider(string path)
        {
            _path = path;
        }

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
                var info = new FileInfo(Combine(_path, subpath));
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
        /// <param name="directoryInfo">The discovered directory, if any</param>
        /// <returns>True if a directory was discovered at the given path</returns>
        public bool TryGetDirectoryInfo(string subpath, out IDirectoryInfo directoryInfo)
        {
            try
            {
                var info = new DirectoryInfo(Combine(_path, subpath));
                if (info.Exists)
                {
                    directoryInfo = new PhysicalDirectoryInfo(info);
                    return true;
                }
            }
            catch (ArgumentException)
            {
            }
            directoryInfo = null;
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