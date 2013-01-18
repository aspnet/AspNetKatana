// -----------------------------------------------------------------------
// <copyright file="IContentTypeProvider.cs" company="Katana contributors">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Owin.StaticFiles.ContentTypes
{
    /// <summary>
    /// Used to look up MIME types given a file path
    /// </summary>
    public interface IContentTypeProvider
    {
        /// <summary>
        /// Given a file path, determine the MIME type
        /// </summary>
        /// <param name="subpath">A file path</param>
        /// <param name="contentType">The resulting MIME type</param>
        /// <returns>True if MIME type could be determined</returns>
        bool TryGetContentType(string subpath, out string contentType);
    }
}