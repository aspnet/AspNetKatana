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
    public class StartParameters
    {
        public string Boot { get; set; }

        public string Server { get; set; }

        public string App { get; set; }
        public string OutputFile { get; set; }
        public int Verbosity { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "The host may contain wildcards not supported by System.Uri")]
        public string Url { get; set; }

        public string Scheme { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Path { get; set; }

        public bool ShowHelp { get; set; }
    }
}
