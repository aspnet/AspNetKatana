// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Owin.Hosting.Tracing
{
    internal partial class DualWriter : TextWriter
    {
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

#else

using FormattingWorkaround = System.Object;

#endif
