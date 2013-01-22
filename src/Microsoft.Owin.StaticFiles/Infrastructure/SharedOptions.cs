// <copyright file="SharedOptions.cs" company="Katana contributors">
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

using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    /// <summary>
    /// Options common to several middleware components
    /// </summary>
    public class SharedOptions
    {
        /// <summary>
        /// Defaults to all request paths and the current physical directory.
        /// </summary>
        public SharedOptions()
        {
            RequestPath = string.Empty;
            FileSystemProvider = new PhysicalFileSystemProvider(".");
        }

        /// <summary>
        /// The request path that maps to static resources
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// The file system used to locate resources
        /// </summary>
        public IFileSystemProvider FileSystemProvider { get; set; }
    }
}
