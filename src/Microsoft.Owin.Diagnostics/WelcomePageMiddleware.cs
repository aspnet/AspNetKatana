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
using Microsoft.Owin.Diagnostics.Views;

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
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

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
            if (!_options.Path.HasValue || _options.Path == request.Path)
            {
                // Dynamically generated for LOC.
                var welcomePage = new WelcomePage();
                welcomePage.Execute(context);
                return TaskHelpers.Completed();
            }

            return Next.Invoke(context);
        }
    }
}
