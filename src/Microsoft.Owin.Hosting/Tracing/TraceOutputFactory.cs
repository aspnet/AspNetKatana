// <copyright file="TraceOutputBinder.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Owin.Hosting.Tracing
{
    /// <summary>
    /// Opens a stream writer for the given file.
    /// </summary>
    public class TraceOutputFactory : ITraceOutputFactory
    {
        /// <summary>
        /// Opens a stream writer for the given file.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        public virtual TextWriter Create(string outputFile)
        {
            return string.IsNullOrWhiteSpace(outputFile)
                ? (TextWriter)new DualWriter(Console.Error)
                : new StreamWriter(outputFile, true);
        }

        // Writes to Debug and the given text writer
        private class DualWriter : TextWriter
        {
            internal DualWriter(TextWriter writer2)
                : base(writer2.FormatProvider)
            {
                Writer2 = writer2;
            }

            private TextWriter Writer2 { get; set; }

            public override System.Text.Encoding Encoding
            {
                get { return Writer2.Encoding; }
            }

            public override void Close()
            {
                Writer2.Close();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Writer2.Dispose();
                }
                base.Dispose(disposing);
            }

            public override void Write(char value)
            {
                Debug.Write(value);
                Writer2.Write(value);
            }

            public override void Write(char[] buffer)
            {
                Debug.Write(new string(buffer));
                Writer2.Write(buffer);
            }

            public override void Write(string value)
            {
                Debug.Write(value);
                Writer2.Write(value);
            }

            public override void Write(char[] buffer, int index, int count)
            {
                Debug.Write(new string(buffer, index, count));
                Writer2.Write(buffer, index, count);
            }

            public override void Flush()
            {
                Debug.Flush();
                Writer2.Flush();
            }
#if !NET40
            public override Task FlushAsync()
            {
                Debug.Flush();
                return Writer2.FlushAsync();
            }

            public override Task WriteAsync(char value)
            {
                Debug.Write(value);
                return Writer2.WriteAsync(value);
            }

            public override Task WriteAsync(string value)
            {
                Debug.Write(value);
                return Writer2.WriteAsync(value);
            }

            public override Task WriteAsync(char[] buffer, int index, int count)
            {
                Debug.Write(new string(buffer, index, count));
                return Writer2.WriteAsync(buffer, index, count);
            }

            public override Task WriteLineAsync()
            {
                Debug.WriteLine(string.Empty);
                return Writer2.WriteLineAsync();
            }

            public override Task WriteLineAsync(char value)
            {
                Debug.WriteLine(value);
                return Writer2.WriteLineAsync(value);
            }

            public override Task WriteLineAsync(string value)
            {
                Debug.WriteLine(value);
                return Writer2.WriteLineAsync(value);
            }

            public override Task WriteLineAsync(char[] buffer, int index, int count)
            {
                Debug.WriteLine(new string(buffer, index, count));
                return Writer2.WriteLineAsync(buffer, index, count);
            }
#endif
        }
    }
}
