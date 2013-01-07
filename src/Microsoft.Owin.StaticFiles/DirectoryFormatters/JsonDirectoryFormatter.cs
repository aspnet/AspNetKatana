// -----------------------------------------------------------------------
// <copyright file="JsonDirectoryFormatter.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    internal class JsonDirectoryFormatter : IDirectoryInfoFormatter
    {
        public string ContentType
        {
            get { return Constants.ApplicationJson; }
        }

        public StringBuilder GenerateContent(string requestPath, FileSystems.IDirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException("directoryInfo");
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("{ ");

            builder.AppendFormat("\"path\": \"{0}\", ", requestPath);

            bool firstItem = true;

            builder.Append("\"subdirectories\": [ ");
            foreach (IDirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                if (!firstItem)
                {
                    builder.Append(", ");
                    firstItem = false;
                }
                builder.AppendFormat("\"name\": \"{0}\"", subdir.Name);
            }
            builder.Append("], ");

            firstItem = true;

            builder.Append("\"files\": [ ");
            foreach (IFileInfo file in directoryInfo.GetFiles())
            {
                if (!firstItem)
                {
                    builder.Append(", ");
                    firstItem = false;
                }
                builder.Append("{ ");
                builder.AppendFormat("\"name\": \"{0}\", ", file.Name);
                builder.AppendFormat("\"length\": {0}, ", file.Length.ToString(CultureInfo.InvariantCulture));
                builder.AppendFormat("\"lastModified\": \"{0}\" ",
                    file.LastModified.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
                builder.Append("} ");
            }
            builder.Append("] ");

            builder.Append("}");
            return builder;
        }
    }
}
