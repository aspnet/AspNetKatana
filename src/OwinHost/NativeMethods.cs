// <copyright file="NativeMethods.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Runtime.InteropServices;

namespace Microsoft.Owin.Hosting.CommandLine
{
    internal static class NativeMethods
    {
        private enum FileType
        {
            Unknown,
            Disk,
            Char,
            Pipe
        }

        private enum StdHandle
        {
            Stdin = -10,
            Stdout = -11,
            Stderr = -12
        }

        internal static bool IsInputRedirected
        {
            get { return FileType.Char != GetFileType(GetStdHandle(StdHandle.Stdin)); }
        }

        [DllImport("kernel32.dll")]
        private static extern FileType GetFileType(IntPtr hdl);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);
    }
}
