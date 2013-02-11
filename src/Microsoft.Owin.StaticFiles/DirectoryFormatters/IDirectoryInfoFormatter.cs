// <copyright file="IDirectoryInfoFormatter.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Text;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    /// <summary>
    /// Generates the view for a directory, depending on a specific content type
    /// </summary>
    public interface IDirectoryInfoFormatter
    {
        /// <summary>
        /// The content-type that describes the output generated
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Generates a view for the given directory
        /// </summary>
        /// <param name="requestPath">The request path</param>
        /// <param name="directoryInfo">The directory to render</param>
        /// <returns>The view, as a StringBuilder</returns>
        StringBuilder GenerateContent(string requestPath, IDirectoryInfo directoryInfo);
    }
}
