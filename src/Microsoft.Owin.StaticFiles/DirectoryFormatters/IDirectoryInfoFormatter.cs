// -----------------------------------------------------------------------
// <copyright file="IDirectoryInfoFormatter.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System.Text;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    // Generates the view for a directory, depending on a specific content type
    public interface IDirectoryInfoFormatter
    {
        string ContentType { get; }

        StringBuilder GenerateContent(string requestPath, IDirectoryInfo directoryInfo);
    }
}
