using System.IO;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    public interface IFileSystemProvider
    {
        bool TryGetFileInfo(string subpath, out IFileInfo fileInfo);
    }
}
