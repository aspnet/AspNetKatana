// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Owin.Host.SystemWeb.Infrastructure
{
    internal static class TraceExtensions
    {
        public static void WriteError(this ITrace trace, string message, Exception error)
        {
            trace.Write(TraceEventType.Error, "{0}\r\n{1}", message, error);
        }

        public static void WriteWarning(this ITrace trace, string message, Exception error)
        {
            trace.Write(TraceEventType.Warning, "{0}\r\n{1}", message, error);
        }
    }
}
