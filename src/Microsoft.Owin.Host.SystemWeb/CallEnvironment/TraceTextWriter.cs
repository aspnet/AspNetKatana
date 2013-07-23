// <copyright file="TraceTextWriter.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Owin.Host.SystemWeb.CallEnvironment
{
    internal class TraceTextWriter : TextWriter
    {
        internal static TraceTextWriter Instance = new TraceTextWriter();

        public TraceTextWriter()
            : base(CultureInfo.InvariantCulture)
        {
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Not for just one reference")]
        [SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api", Justification = "We care calling the equivalent Debugging.Log when it's enabled.")]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern void OutputDebugString(string message);

        public override void Write(char value)
        {
            Write(value.ToString());
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }

        public override void WriteLine(string value)
        {
            Write(value + Environment.NewLine);
        }

        public override void Write(string value)
        {
            if (Debugger.IsLogging())
            {
                Debugger.Log(0, null, value);
            }
            else
            {
                OutputDebugString(value ?? string.Empty);
            }
        }
    }
}
