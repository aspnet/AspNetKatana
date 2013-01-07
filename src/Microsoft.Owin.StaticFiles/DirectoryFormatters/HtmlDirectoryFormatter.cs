// -----------------------------------------------------------------------
// <copyright file="HtmlDirectoryFormatter.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    internal class HtmlDirectoryFormatter : IDirectoryInfoFormatter
    {
        public string ContentType
        {
            get { return Constants.TextHtml; }
        }

        public StringBuilder GenerateContent(string requestPath, IDirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException("directoryInfo");
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("<html><body>");

            builder.AppendFormat("<h1>{0}</h1>", requestPath);
            builder.Append("<br>");

            foreach (IDirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                builder.AppendFormat("<a href=\"./{0}/\">{0}/</a><br>", subdir.Name);
            }
            builder.Append("<br>");

            foreach (IFileInfo file in directoryInfo.GetFiles())
            {
                builder.AppendFormat("<a href=\"./{0}\">{0}</a>, {1}, {2}<br>", file.Name, file.Length, file.LastModified);
            }
            builder.Append("<br>");

            builder.Append("</body></html>");
            return builder;
        }
    }
}
