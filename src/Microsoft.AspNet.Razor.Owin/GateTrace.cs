// -----------------------------------------------------------------------
// <copyright file="GateTrace.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gate;

namespace Microsoft.AspNet.Razor.Owin
{
    public class GateTrace : ITrace
    {
        public static readonly ITrace Global = new GateTrace();

        private GateTrace()
        {
            Request = new Request();
            RequestId = -1;
        }

        public GateTrace(Request request, long id)
        {
            RequestId = id;
            Request = request;
        }

        public long RequestId { get; private set; }
        public Request Request { get; private set; }

        public IDisposable StartTrace()
        {
            TraceMessage("Trace Started");
            return new DisposableAction(() => TraceMessage("Trace Complete"));
        }

        public void WriteLine(string format, params object[] args)
        {
            TraceMessage(format, args);
        }

        private void TraceMessage(string format, params object[] args)
        {
            string message = String.Format(format, args);
            if (RequestId != -1)
            {
                Request.TraceOutput.WriteLine("[{2}][Razor #{0}]: {1}", RequestId, message, DateTime.Now.ToString("HH:mm:ss.fff"));
            }
            else
            {
                Trace.WriteLine(String.Format("[{1}][Razor Global]: {0}", message, DateTime.Now.ToString("HH:mm:ss.fff")));
            }
        }
    }
}
