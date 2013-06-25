// <copyright file="WelcomePageMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.Diagnostics
{
    /// <summary>
    /// This middleware provides a default web page for new applications.
    /// </summary>
    public class WelcomePageMiddleware : OwinMiddleware
    {
        private readonly WelcomePageOptions _options;

        /// <summary>
        /// Creates a default web page for new applications.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public WelcomePageMiddleware(OwinMiddleware next, WelcomePageOptions options)
            : base(next)
        {
            _options = options;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IOwinRequest request = context.Request;
            if (string.IsNullOrEmpty(_options.Path) || string.Equals(request.Path, _options.Path, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Make it pretty
                IOwinResponse response = context.Response;
                response.ContentType = "text/html";

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<html>")

                .AppendLine("<head>")
                .AppendLine("<title>")
                .AppendLine("Welcome")
                .AppendLine("</title>")
                .AppendLine("</head>")

                .AppendLine("<body>")
                .AppendLine("<H1>Welcome</H1>")
                .AppendLine("<p>You have reached the default application page.</p>")
                .AppendLine("<H4>Additional Resources:</H4>")
                .AppendLine("<ul>")
                .AppendLine("<li><a href=\"http://katanaproject.codeplex.com/\">Katana Project</a>")
                .AppendLine("<li><a href=\"http://www.owin.org/\">owin.org</a>")
                .AppendLine("</ul>")
                .AppendLine("</body>")
                .AppendLine("</html>");

                response.WriteAsync(builder.ToString());
            }

            return Next.Invoke(context);
        }
    }
}
