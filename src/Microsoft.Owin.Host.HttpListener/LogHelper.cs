// <copyright file="LogHelper.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.HttpListener
{
    using LoggerFactoryFunc = Func<string, Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool>>;
    using LoggerFunc = Func<TraceEventType, int, object, Exception, Func<object, Exception, string>, bool>;

    internal static class LogHelper
    {
        private static readonly Func<object, Exception, string> LogState = 
            (state, error) => Convert.ToString(state, CultureInfo.CurrentCulture);
        private static readonly Func<object, Exception, string> LogStateAndError =
            (state, error) => string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", state, error);

        internal static LoggerFunc CreateLogger(LoggerFactoryFunc factory, Type type)
        {
            if (factory == null)
            {
                return null;
            }

            return factory(type.FullName);
        }

        internal static void LogInfo(LoggerFunc logger, string data)
        {
            if (logger == null)
            {
                Debug.WriteLine(data);
            }
            else
            {
                logger(TraceEventType.Information, 0, data, null, LogState);
            }
        }

        internal static void LogException(LoggerFunc logger, string location, Exception exception)
        {
            if (logger == null)
            {
                Debug.WriteLine(exception);
            }
            else
            {
                logger(TraceEventType.Error, 0, location, exception, LogStateAndError);
            }
        }
    }
}
