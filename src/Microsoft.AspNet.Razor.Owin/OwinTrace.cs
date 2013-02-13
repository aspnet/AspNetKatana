// <copyright file="OwinTrace.cs" company="Microsoft Open Technologies, Inc.">
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
using Gate;
using Microsoft.AspNet.Razor.Owin.Execution;

namespace Microsoft.AspNet.Razor.Owin
{
    public class OwinTrace : ITrace
    {
        public static readonly ITrace Global = new OwinTrace();

        private OwinTrace()
        {
            Request = null;
            RequestId = -1;
        }

        public OwinTrace(IRazorRequest request, long id)
        {
            RequestId = id;
            Request = request;
        }

        public long RequestId { get; private set; }
        public IRazorRequest Request { get; private set; }

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
