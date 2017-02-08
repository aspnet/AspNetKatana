// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
