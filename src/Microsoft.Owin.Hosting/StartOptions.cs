// <copyright file="StartParameters.cs" company="Katana contributors">
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
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Hosting
{
    [Serializable]
    public class StartOptions
    {
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "The host may contain wildcards not supported by System.Uri")]
        public string Url { get; set; }

        /// <summary>
        /// Name of the assembly containing the http server implementation
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Name of the assembly replacing the startup logic
        /// </summary>
        public string Boot { get; set; }

        /// <summary>
        /// Value used to locate and load web application startup routine
        /// </summary>
        public string App { get; set; }

        /// <summary>
        /// Optional file name used to capture text-oriented trace messages
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// Optional level of text-oriented trace message verbosity
        /// 0 can be interpreted as default (warning, error, or fatal)
        /// 1 can be interpreted as informational (message or info)
        /// 2 or more can be interpreted as all information (verbose or debug)
        /// </summary>
        public int Verbosity { get; set; }
    }
}
