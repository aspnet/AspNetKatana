// <copyright file="StartOptions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Hosting
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class StartOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public StartOptions()
        {
            Urls = new List<string>();
            Settings = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public StartOptions(string url)
            : this()
        {
            Urls.Add(url);
        }

        /// <summary>
        /// A list of url prefixes to listen on. Overrides port.
        /// </summary>
        public IList<string> Urls { get; private set; }

        /// <summary>
        /// A port to listen on.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Parameter to locate and load web application startup routine
        /// </summary>
        public string AppStartup { get; set; }

        /// <summary>
        /// Name of the assembly containing the http server implementation
        /// </summary>
        public string ServerFactory { get; set; }

        /// <summary>
        /// Name of the assembly providing custom domain startup logic
        /// </summary>
        public string Boot { get; set; }

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

        /// <summary>
        /// Optional settings used to override service types and other defaults
        /// </summary>
        public IDictionary<string, string> Settings { get; private set; }

        /// <summary>
        /// Optional directory containing web application. Defaults to the
        /// current directory.
        /// </summary>
        public string Directory { get; set; }
    }
}
