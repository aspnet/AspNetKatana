// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Owin.FileSystems
{
    

    /// <summary>
    ///     Looks up files using embedded resources in the specified assembly
    /// </summary>
    public class EmbeddedResourceFileSystem : IFileSystem
    {
        private readonly Assembly _assembly;
        private readonly string _baseNamespace;
        private readonly DateTime _lastModified;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedResourceFileSystem" /> class using the calling
        ///     assembly and empty base namespace.
        /// </summary>
        public EmbeddedResourceFileSystem()
            : this(Assembly.GetCallingAssembly())
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedResourceFileSystem" /> class using the specified
        ///     assembly and empty base namespace.
        /// </summary>
        /// <param name="assembly"></param>
        public EmbeddedResourceFileSystem(Assembly assembly)
            : this(assembly, string.Empty)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedResourceFileSystem" /> class using the calling
        ///     assembly and specified base namespace.
        /// </summary>
        /// <param name="baseNamespace">The base namespace that contains the embedded resources.</param>
        public EmbeddedResourceFileSystem(string baseNamespace)
            : this(Assembly.GetCallingAssembly(), baseNamespace)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedResourceFileSystem" /> class using the specified
        ///     assembly and root namespace.
        /// </summary>
        /// <param name="assembly">The assembly that contains the embedded resources.</param>
        /// <param name="baseNamespace">The base namespace that contains the embedded resources.</param>
        public EmbeddedResourceFileSystem(Assembly assembly, string baseNamespace)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            _baseNamespace = baseNamespace ?? string.Empty;
            _assembly = assembly;
            _lastModified = new FileInfo(assembly.Location).LastWriteTime;
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            string resourcePath = string.Concat(_baseNamespace, ".", subpath.Replace("/", "."));
            if (_assembly.GetManifestResourceInfo(resourcePath) == null)
            {
                fileInfo = null;
                return false;
            }
            fileInfo = new EmbeddedResourceFileInfo(_assembly, resourcePath, _lastModified);
            return true;
        }

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            throw new NotSupportedException("Directory functionality not supported.");
        }

        private class EmbeddedResourceFileInfo : IFileInfo
        {
            private readonly DateTime _lastModified;
            private readonly Stream _resourceStream;

            public EmbeddedResourceFileInfo(Assembly assembly, string resourcePath, DateTime lastModified)
            {
                _lastModified = lastModified;
                _resourceStream = assembly.GetManifestResourceStream(resourcePath);
            }

            public Stream CreateReadStream()
            {
                return _resourceStream;
            }

            public long Length
            {
                get { return _resourceStream.Length; }
            }

            public string PhysicalPath
            {
                get { return null; } //TODO What should be returned here?
            }

            public string Name
            {
                get { return null; } //TODO What should be returned here?
            }

            public DateTime LastModified
            {
                get { return _lastModified; }
            }

            public bool IsDirectory
            {
                get { return false; }
            }
        }
    }
}
