// -----------------------------------------------------------------------
// <copyright file="SharedOptionsBase.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Owin.StaticFiles.FileSystems;

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
            SharedOptions = sharedOptions;
        }

        /// <summary>
        /// Options common to several middleware components
        /// </summary>
        protected SharedOptions SharedOptions { get; private set; }

        /// <summary>
        /// The request path that maps to static resources
        /// </summary>
        public string RequestPath
        {
            get { return SharedOptions.RequestPath; }
            set { SharedOptions.RequestPath = value; }
        }

        /// <summary>
        /// The file system used to locate resources
        /// </summary>
        public IFileSystemProvider FileSystemProvider
        {
            get { return SharedOptions.FileSystemProvider; }
            set { SharedOptions.FileSystemProvider = value; }
        }

        /// <summary>
        /// Sets the request path
        /// </summary>
        /// <param name="path">The request path</param>
        /// <returns>this</returns>
        public T WithRequestPath(string path)
        {
            RequestPath = path;
            return (T)(object)this;
        }

        /// <summary>
        /// Sets the file system
        /// </summary>
        /// <param name="fileSystemProvider">The file system</param>
        /// <returns>this</returns>
        public T WithFileSystemProvider(IFileSystemProvider fileSystemProvider)
        {
            FileSystemProvider = fileSystemProvider;
            return (T)(object)this;
        }

        /// <summary>
        /// Sets a physical file system at the given disk path
        /// </summary>
        /// <param name="path">The root disk path</param>
        /// <returns>this</returns>
        public T WithPhysicalPath(string path)
        {
            return WithFileSystemProvider(new PhysicalFileSystemProvider(path));
        }
    }
}
