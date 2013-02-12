// <copyright file="IFileInfo.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;

namespace Microsoft.Owin.FileSystems
{
    /// <summary>
    /// Represents a file in the given file system.
    /// </summary>
    public interface IFileInfo
    {
        /// <summary>
        /// The length of the file in bytes
        /// </summary>
        long Length { get; }

        /// <summary>
        /// The path to the file, including the file name
        /// </summary>
        string PhysicalPath { get; }

        /// <summary>
        /// The name of the file
        /// </summary>
        string Name { get; }

        /// <summary>
        /// When the file was last modified
        /// </summary>
        DateTime LastModified { get; }

        Stream CreateReadStream();
    }
}
