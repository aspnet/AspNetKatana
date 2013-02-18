// <copyright file="HtmlDirectoryFormatter.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    internal class HtmlDirectoryFormatter : IDirectoryInfoFormatter
    {
        public string ContentType
        {
            get { return Constants.TextHtml; }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Owin.StaticFiles.DirectoryFormatters.HtmlDirectoryFormatter.Encode(System.String)", Justification = "By design")]
        public StringBuilder GenerateContent(string requestPath, IEnumerable<IFileInfo> contents)
        {
            if (requestPath == null)
            {
                throw new ArgumentNullException("requestPath");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }

            var builder = new StringBuilder();

            builder.AppendFormat(
                @"<!DOCTYPE html>
<html>
<head>
  <title>Index of {0}</title>
  <style>
    body {{
        font-family: ""Segoe UI"", ""Segoe WP"", ""Helvetica Neue"", 'RobotoRegular', sans-serif;
        font-size: 14px;}}
    header h1 {{
        font-family: ""Segoe UI Light"", ""Helvetica Neue"", 'RobotoLight', ""Segoe UI"", ""Segoe WP"", sans-serif;
        font-size: 28px;
        font-weight: 100;
        margin-top: 5px;
        margin-bottom: 0px;}}
    #index {{ 
        border-collapse: separate; 
        border-spacing: 0; 
        margin: 0 0 20px; }}
    #index th {{
        vertical-align: bottom;
        padding: 10px 5px 5px 5px;
        font-weight: 400;
        color: #a0a0a0;
        text-align: left;
    }}
    #index td {{
        padding: 3px 10px;
    }}
    #index th, #index td {{
        border-right: 1px #ddd solid;
        border-bottom: 1px #ddd solid;
        border-left: 1px transparent solid;
        border-top: 1px transparent solid;
        box-sizing: border-box;
    }}
    #index th:last-child, #index td:last-child {{
        border-right: 1px transparent solid;
    }}
    #index td.length {{ text-align:right; }}
    a {{ color:#1ba1e2;text-decoration:none; }}
    a:hover {{ color:#13709e;text-decoration:underline; }}
  </style>
</head>
<body>
  <section id=""main"">
    <header><h1>Index of <a href=""/"">/</a>", Encode(requestPath));

            string cumulativePath = "/";
            foreach (var segment in requestPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                cumulativePath = cumulativePath + segment + "/";
                builder.AppendFormat(@"<a href=""{0}"">{1}/</a>",
                    Encode(cumulativePath), Encode(segment));
            }

            builder.Append(
                @"</h1></header>
    <table id=""index"">
    <thead>
      <tr><th>Name</th><th>Size</th><th>Last Modified</th></tr>
    </thead>
    <tbody>");

            foreach (var subdir in contents.Where(info => info.IsDirectory))
            {
                builder.AppendFormat(@"
      <tr class=""directory"">
        <td class=""name""><a href=""{0}/"">{0}/</a></td>
        <td></td>
        <td class=""modified"">{1}</td>
      </tr>",
                    Encode(subdir.Name),
                    subdir.LastModified);
            }

            foreach (var file in contents.Where(info => !info.IsDirectory))
            {
                builder.AppendFormat(@"
      <tr class=""file"">
        <td class=""name""><a href=""{0}"">{0}</a></td>
        <td class=""length"">{1}</td>
        <td class=""modified"">{2}</td>
      </tr>",
                    Encode(file.Name),
                    file.Length,
                    file.LastModified);
            }

            builder.Append(@"
    </tbody>
    </table>
  </section>
</body>
</html>");
            return builder;
        }

        private static string Encode(string text)
        {
            return text;
        }
    }
}
