// -----------------------------------------------------------------------
// <copyright file="PlainTextDirectoryFormatter.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    internal class PlainTextDirectoryFormatter : IDirectoryInfoFormatter
    {
        public string ContentType
        {
            get { return Constants.TextPlain; }
        }

        public StringBuilder GenerateContent(string requestPath, IDirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException("directoryInfo");
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}\r\n", requestPath);
            builder.Append("\r\n");

            foreach (IDirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                builder.AppendFormat("{0}/\r\n", subdir.Name);
            }
            builder.Append("\r\n");

            foreach (IFileInfo file in directoryInfo.GetFiles())
            {
                builder.AppendFormat("{0}, {1}, {2}\r\n", file.Name, file.Length, file.LastModified);
            }
            builder.Append("\r\n");

            return builder;
        }
    }
}
