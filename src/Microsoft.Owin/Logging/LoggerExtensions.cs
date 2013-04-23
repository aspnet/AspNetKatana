// <copyright file="TraceExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Logging
{
    public static class LoggerExtensions
    {
        private static readonly Func<object, Exception, string> TheMessage = (message, error) => (string)message;
        private static readonly Func<object, Exception, string> TheMessageAndError = (message, error) => string.Format("{0}\r\n{1}", message, error);

        public static bool IsEnabled(this ILogger logger, TraceEventType eventType)
        {
            return logger.WriteCore(eventType, 0, null, null, null);
        }

        public static void WriteVerbose(this ILogger logger, string message)
        {
            logger.WriteCore(TraceEventType.Verbose, 0, message, null, TheMessage);
        }

        public static void WriteInformation(this ILogger logger, string message)
        {
            logger.WriteCore(TraceEventType.Information, 0, message, null, TheMessage);
        }

        public static void WriteWarning(this ILogger logger, string message)
        {
            logger.WriteCore(TraceEventType.Warning, 0, message, null, TheMessage);
        }

        public static void WriteWarning(this ILogger logger, string message, Exception error)
        {
            logger.WriteCore(TraceEventType.Warning, 0, message, error, TheMessageAndError);
        }

        public static void WriteError(this ILogger logger, string message)
        {
            logger.WriteCore(TraceEventType.Error, 0, message, null, TheMessage);
        }

        public static void WriteError(this ILogger logger, string message, Exception error)
        {
            logger.WriteCore(TraceEventType.Error, 0, message, error, TheMessageAndError);
        }

        public static void WriteCritical(this ILogger logger, string message)
        {
            logger.WriteCore(TraceEventType.Error, 0, message, null, TheMessage);
        }

        public static void WriteCritical(this ILogger logger, string message, Exception error)
        {
            logger.WriteCore(TraceEventType.Error, 0, message, error, TheMessageAndError);
        }
    }
}
