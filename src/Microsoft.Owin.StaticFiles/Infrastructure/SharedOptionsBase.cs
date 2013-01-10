using Microsoft.Owin.StaticFiles.FileSystems;

// ReSharper disable InconsistentNaming

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    public abstract class SharedOptionsBase<T>
    {
        protected readonly SharedOptions _sharedOptions;

        protected SharedOptionsBase(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions;
        }

        public string RequestPath
        {
            get { return _sharedOptions.RequestPath; }
            set { _sharedOptions.RequestPath = value; }
        }

        public IFileSystemProvider FileSystemProvider
        {
            get { return _sharedOptions.FileSystemProvider; }
            set { _sharedOptions.FileSystemProvider = value; }
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