// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Owin.Logging
{
    internal class DiagnosticsLogger : ILogger
    {
        private readonly TraceSource _traceSource;

        public DiagnosticsLogger(TraceSource traceSource)
        {
            _traceSource = traceSource;
        }

        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!_traceSource.Switch.ShouldTrace(eventType))
            {
                return false;
            }
            else if (formatter != null)
            {
                _traceSource.TraceEvent(eventType, eventId, formatter(state, exception));
            }
            return true;
        }
    }
}
