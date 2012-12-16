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

        class PhysicalFileInfo : IFileInfo
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
        }

    }
}