// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Owin.Hosting.Tracing
{
    internal class DualWriter : TextWriter
    {
        private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;
        
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

        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Justification = "Not for just one reference")]
        [SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api", Justification = "We care calling the equivalent Debugging.Log when it's enabled.")]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern void OutputDebugString(string message);

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

        private static void InternalWrite(string message)
        {
            if (Debugger.IsLogging())
            {
                Debugger.Log(0, null, message);
            }
            else
            {
                if (!IsMono)
                {
                    OutputDebugString(message ?? String.Empty);
                }
                else
                {
                    Debug.Write(message ?? String.Empty);
                }
            }
        }

        public override void Write(char value)
        {
            InternalWrite(value.ToString());
            Writer2.Write(value);
        }

        public override void Write(char[] buffer)
        {
            InternalWrite(new string(buffer));
            Writer2.Write(buffer);
        }

        public override void Write(string value)
        {
            InternalWrite(value);
            Writer2.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            InternalWrite(new string(buffer, index, count));
            Writer2.Write(buffer, index, count);
        }

        public override void Flush()
        {
            // InternalFlush
            Writer2.Flush();
        }
        public override Task FlushAsync()
        {
            // InternalFlush
            return Writer2.FlushAsync();
        }

        public override Task WriteAsync(char value)
        {
            InternalWrite(value.ToString());
            return Writer2.WriteAsync(value);
        }

        public override Task WriteAsync(string value)
        {
            InternalWrite(value);
            return Writer2.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            InternalWrite(new string(buffer, index, count));
            return Writer2.WriteAsync(buffer, index, count);
        }

        public override Task WriteLineAsync()
        {
            InternalWrite(Environment.NewLine);
            return Writer2.WriteLineAsync();
        }

        public override Task WriteLineAsync(char value)
        {
            InternalWrite(value + Environment.NewLine);
            return Writer2.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(string value)
        {
            InternalWrite(value + Environment.NewLine);
            return Writer2.WriteLineAsync(value);
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            InternalWrite(new string(buffer, index, count) + Environment.NewLine);
            return Writer2.WriteLineAsync(buffer, index, count);
        }
    }
}
