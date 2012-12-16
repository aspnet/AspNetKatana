namespace Microsoft.Owin.StaticFiles.ContentTypes
{
    public interface IContentTypeProvider
    {
        bool TryGetContentType(string subpath, out string contentType);
    }
}