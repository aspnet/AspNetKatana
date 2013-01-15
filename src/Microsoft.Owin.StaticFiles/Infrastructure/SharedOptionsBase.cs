// -----------------------------------------------------------------------
// <copyright file="SharedOptionsBase.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    public abstract class SharedOptionsBase<T>
    {
        protected SharedOptionsBase(SharedOptions sharedOptions)
        {
            SharedOptions = sharedOptions;
        }

        protected SharedOptions SharedOptions { get; private set; }

        public string RequestPath
        {
            get { return SharedOptions.RequestPath; }
            set { SharedOptions.RequestPath = value; }
        }

        public IFileSystemProvider FileSystemProvider
        {
            get { return SharedOptions.FileSystemProvider; }
            set { SharedOptions.FileSystemProvider = value; }
        }

        public T WithRequestPath(string path)
        {
            RequestPath = path;
            return (T)(object)this;
        }

        public T WithFileSystemProvider(IFileSystemProvider fileSystemProvider)
        {
            FileSystemProvider = fileSystemProvider;
            return (T)(object)this;
        }

        public T WithPhysicalPath(string path)
        {
            return WithFileSystemProvider(new PhysicalFileSystemProvider(path));
        }
    }
}
