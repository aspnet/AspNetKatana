// <copyright file="DiagnosticsLogger.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Globalization;

namespace Microsoft.Owin.Logging
{
    internal class DiagnosticsLogger : ILogger
    {
        private static readonly Func<object, Exception, string> TheState = (state, error) => Convert.ToString(state, CultureInfo.CurrentCulture);
        private static readonly Func<object, Exception, string> TheError = (state, error) => Convert.ToString(error, CultureInfo.CurrentCulture);
        private static readonly Func<object, Exception, string> TheStateAndError = (state, error) => string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", state, error);

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
            else if (exception == null)
            {
                _traceSource.TraceEvent(eventType, eventId, TheState(state, exception));
            }
            else if (state == null)
            {
                _traceSource.TraceEvent(eventType, eventId, TheError(state, exception));
            }
            else
            {
                _traceSource.TraceEvent(eventType, eventId, TheStateAndError(state, exception));
            }
            return true;
        }
    }
}
