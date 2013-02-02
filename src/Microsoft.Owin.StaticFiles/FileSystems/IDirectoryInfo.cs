// <copyright file="IDirectoryInfo.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.StaticFiles.FileSystems
{
    /// <summary>
    /// Represents a directory of files and directories.
    /// </summary>
    public interface IDirectoryInfo
    {
        /// <summary>
        /// The path to this directory, including its name
        /// </summary>
        string PhysicalPath { get; }

        /// <summary>
        /// The name of this directory
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Last time this directory was modified
        /// </summary>
        DateTime LastModified { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Any sub directories in the current directory</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        IEnumerable<IDirectoryInfo> GetDirectories();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Any files in the current directory</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        IEnumerable<IFileInfo> GetFiles();
    }
}
