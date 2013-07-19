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

using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        public override void Write(char value)
        {
            Debug.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Debug.Write(new string(buffer, index, count));
        }

        public override void Write(string value)
        {
            Debug.Write(value);
        }

        public override void WriteLine(string value)
        {
            Debug.WriteLine(value);
        }
    }
}
