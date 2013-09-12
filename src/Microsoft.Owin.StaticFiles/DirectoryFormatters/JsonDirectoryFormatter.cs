// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    internal class JsonDirectoryFormatter : IDirectoryInfoFormatter
    {
        public string ContentType
        {
            get { return Constants.ApplicationJson; }
        }

        public StringBuilder GenerateContent(PathString requestPath, IEnumerable<IFileInfo> contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            var builder = new StringBuilder();
            builder.Append("{ ");

            builder.AppendFormat("\"path\": \"{0}\", ", requestPath);

            bool firstItem = true;

            builder.Append("\"subdirectories\": [ ");
            foreach (var subdir in contents.Where(info => info.IsDirectory))
            {
                if (!firstItem)
                {
                    builder.Append(", ");
                }
                else
                {
                    firstItem = false;
                }
                builder.Append("{");
                builder.AppendFormat("\"name\": \"{0}\"", subdir.Name);
                builder.Append("}");
            }
            builder.Append("], ");

            firstItem = true;

            builder.Append("\"files\": [ ");
            foreach (var file in contents.Where(info => !info.IsDirectory))
            {
                if (!firstItem)
                {
                    builder.Append(", ");
                }
                else
                {
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
