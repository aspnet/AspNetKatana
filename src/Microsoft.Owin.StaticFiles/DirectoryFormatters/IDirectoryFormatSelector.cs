// <copyright file="IDirectoryFormatSelector.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;

namespace Microsoft.Owin.StaticFiles.DirectoryFormatters
{
    /// <summary>
    /// Used to determine which output formatter should be used for a given request
    /// </summary>
    public interface IDirectoryFormatSelector
    {
        /// <summary>
        /// Look up a directory view formatter given the request
        /// </summary>
        /// <param name="environment">The request environment</param>
        /// <param name="formatter">The determined formatter, if any</param>
        /// <returns>True if a formatter was determined</returns>
        bool TryDetermineFormatter(IDictionary<string, object> environment, out IDirectoryInfoFormatter formatter);
    }
}
