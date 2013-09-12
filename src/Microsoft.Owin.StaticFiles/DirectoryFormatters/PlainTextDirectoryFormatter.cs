// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    internal class PlainTextDirectoryFormatter : IDirectoryInfoFormatter
    {
        public string ContentType
        {
            get { return Constants.TextPlain; }
        }

        public StringBuilder GenerateContent(PathString requestPath, IEnumerable<IFileInfo> contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            var builder = new StringBuilder();
            builder.AppendFormat("{0}\r\n", requestPath);
            builder.Append("\r\n");

            foreach (var subdir in contents.Where(info => info.IsDirectory))
            {
                builder.AppendFormat("{0}/\r\n", subdir.Name);
            }
            builder.Append("\r\n");

            foreach (var file in contents.Where(info => !info.IsDirectory))
            {
                builder.AppendFormat("{0}, {1}, {2}\r\n", file.Name, file.Length, file.LastModified);
            }
            builder.Append("\r\n");

            return builder;
        }
    }
}
