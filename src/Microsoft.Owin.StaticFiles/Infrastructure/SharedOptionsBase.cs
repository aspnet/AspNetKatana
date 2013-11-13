// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles.Filters;

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    /// <summary>
    /// Options common to several middleware components
    /// </summary>
    /// <typeparam name="T">The type of the subclass</typeparam>
    public abstract class SharedOptionsBase<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sharedOptions"></param>
        protected SharedOptionsBase(SharedOptions sharedOptions)
        {
            if (sharedOptions == null)
            {
                throw new ArgumentNullException("sharedOptions");
            }

            SharedOptions = sharedOptions;
        }

        /// <summary>
        /// Options common to several middleware components
        /// </summary>
        protected SharedOptions SharedOptions { get; private set; }

        /// <summary>
        /// The relative request path that maps to static resources.
        /// </summary>
        public PathString RequestPath
        {
            get { return SharedOptions.RequestPath; }
            set { SharedOptions.RequestPath = value; }
        }

        /// <summary>
        /// The file system used to locate resources
        /// </summary>
        public IFileSystem FileSystem
        {
            get { return SharedOptions.FileSystem; }
            set { SharedOptions.FileSystem = value; }
        }

        /// <summary>
        /// Invoked on each request to determine if the identified file or directory should be served.
        /// All files are served if this is null.
        /// </summary>
        public IRequestFilter Filter
        {
            get { return SharedOptions.Filter; }
            set { SharedOptions.Filter = value; }
        }

        /// <summary>
        /// Sets the request path
        /// </summary>
        /// <param name="path">The relative request path.</param>
        /// <returns>this</returns>
        public T WithRequestPath(string path)
        {
            RequestPath = new PathString(path);
            return (T)(object)this;
        }

        /// <summary>
        /// Sets the file system
        /// </summary>
        /// <param name="fileSystem">The file system</param>
        /// <returns>this</returns>
        public T WithFileSystem(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
            return (T)(object)this;
        }

        /// <summary>
        /// Sets a physical file system at the given disk path
        /// </summary>
        /// <param name="path">The root disk path</param>
        /// <returns>this</returns>
        public T WithPhysicalPath(string path)
        {
            return WithFileSystem(new PhysicalFileSystem(path));
        }
    }
}
