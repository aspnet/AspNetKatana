using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.AspNet.Owin.CallEnvironment
{
    public class TraceTextWriter : TextWriter
    {
        public static TraceTextWriter Instance = new TraceTextWriter();

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public override void Write(string value)
        {
            Trace.Write(value);
        }

        public override void WriteLine(string value)
        {
            Trace.WriteLine(value);
        }
    }
}
