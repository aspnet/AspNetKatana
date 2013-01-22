// <copyright file="PlainTextDirectoryFormatter.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

            var builder = new StringBuilder();
            builder.AppendFormat("{0}\r\n", requestPath);
            builder.Append("\r\n");

            foreach (var subdir in directoryInfo.GetDirectories())
            {
                builder.AppendFormat("{0}/\r\n", subdir.Name);
            }
            builder.Append("\r\n");

            foreach (var file in directoryInfo.GetFiles())
            {
                builder.AppendFormat("{0}, {1}, {2}\r\n", file.Name, file.Length, file.LastModified);
            }
            builder.Append("\r\n");

            return builder;
        }
    }
}
