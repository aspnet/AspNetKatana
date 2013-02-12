// <copyright file="PlainTextDirectoryFormatter.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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

        public StringBuilder GenerateContent(string requestPath, IEnumerable<IFileInfo> contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            var builder = new StringBuilder();
            builder.AppendFormat("{0}\r\n", requestPath);
            builder.Append("\r\n");

            foreach (var subdir in contents.Where(info => info.Length == -1))
            {
                builder.AppendFormat("{0}/\r\n", subdir.Name);
            }
            builder.Append("\r\n");

            foreach (var file in contents.Where(info => info.Length != -1))
            {
                builder.AppendFormat("{0}, {1}, {2}\r\n", file.Name, file.Length, file.LastModified);
            }
            builder.Append("\r\n");

            return builder;
        }
    }
}
