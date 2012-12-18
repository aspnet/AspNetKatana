// -----------------------------------------------------------------------
// <copyright file="IContentTypeProvider.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Owin.StaticFiles.ContentTypes
{
    public interface IContentTypeProvider
    {
        bool TryGetContentType(string subpath, out string contentType);
    }
}