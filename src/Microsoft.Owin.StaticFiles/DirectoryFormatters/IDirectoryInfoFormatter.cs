// -----------------------------------------------------------------------
// <copyright file="IDirectoryInfoFormatter.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    /// <summary>
    /// Generates the view for a directory, depending on a specific content type
    /// </summary>
    public interface IDirectoryInfoFormatter
    {
        /// <summary>
        /// The content-type that describes the output generated
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Generates a view for the given directory
        /// </summary>
        /// <param name="requestPath">The request path</param>
        /// <param name="directoryInfo">The directory to render</param>
        /// <returns>The view, as a StringBuilder</returns>
        StringBuilder GenerateContent(string requestPath, IDirectoryInfo directoryInfo);
    }
}
