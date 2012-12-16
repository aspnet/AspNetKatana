namespace Microsoft.Owin.StaticFiles.FileSystems
{
    public interface IFileInfo
    {
        long Length { get; }
        string PhysicalPath { get; }
    }
}