// <copyright file="Utils.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class Utils
    {
        // Converts path value to a normal form.
        // Null values are treated as string.empty.
        // A path segment is always accompanied by it's leading slash.
        // A root path is string.empty
        internal static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path ?? string.Empty;
            }
            if (path.Length == 1)
            {
                return path[0] == '/' ? string.Empty : '/' + path;
            }
            return path[0] == '/' ? path : '/' + path;
        }
    }
}
