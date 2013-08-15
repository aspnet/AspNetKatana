// <copyright file="DiagnosticsPageOptions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Diagnostics
{
    /// <summary>
    /// Options for the ErrorPageMiddleware
    /// </summary>
    public class ErrorPageOptions
    {
        /// <summary>
        /// Create an instance with the default options settings.
        /// </summary>
        public ErrorPageOptions()
        {
            SourceCodeLineCount = 6;
        }

        /// <summary>
        /// Enables the display of exception types, messages, and stack traces.
        /// </summary>
        public bool? ShowExceptionDetails { get; set; }

        /// <summary>
        /// Enabled the display of local source code around exception stack frames.
        /// </summary>
        public bool? ShowSourceCode { get; set; }

        /// <summary>
        /// Determines how many lines of code to include before and after the line of code
        /// present in an exception's stack frame. Only applies when symbols are available and 
        /// source code referenced by the exception stack trace is present on the server.
        /// </summary>
        public int SourceCodeLineCount { get; set; }

        /// <summary>
        /// Enables the enumeration of any parsed query values.
        /// </summary>
        public bool? ShowQuery { get; set; }

        /// <summary>
        /// Enables the enumeration of any parsed request cookies.
        /// </summary>
        public bool? ShowCookies { get; set; }

        /// <summary>
        /// Enables the enumeration of the request headers.
        /// </summary>
        public bool? ShowHeaders { get; set; }

        /// <summary>
        /// Enables the enumeration of the OWIN environment values.
        /// </summary>
        public bool? ShowEnvironment { get; set; }
    }
}
