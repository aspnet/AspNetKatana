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
using System.Globalization;
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
                IOwinResponse response = context.Response;
                response.ContentType = "text/html; charset=utf-8";
                string welcomePageFormat = Resources.WelcomePage;
                string localizedWelcomePage = string.Format(CultureInfo.CurrentUICulture, welcomePageFormat, 
                    Resources.WelcomeTitle, Resources.WelcomeHeader, Resources.WelcomeStarted, Resources.WelcomeLearnOwin,
                    Resources.WelcomeLearnMicrosoftOwin);
                byte[] bytes = Encoding.UTF8.GetBytes(localizedWelcomePage);
                response.ContentLength = bytes.Length;
                return response.WriteAsync(bytes);
            }

            return Next.Invoke(context);
        }
    }
}
