// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal class DefaultTrace : ITrace
    {
        private readonly TraceSource _traceSource;

        public DefaultTrace(TraceSource traceSource)
        {
            _traceSource = traceSource;
        }

        public void Write(TraceEventType eventType, string format, params object[] args)
        {
            _traceSource.TraceEvent(eventType, 0, format, args);
        }
    }
}
