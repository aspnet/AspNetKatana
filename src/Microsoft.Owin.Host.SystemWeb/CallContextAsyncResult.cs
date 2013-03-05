// <copyright file="CallContextAsyncResult.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal class CallContextAsyncResult : IAsyncResult
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.CallContextAsyncResult";

        private static readonly AsyncCallback NoopAsyncCallback = ar => { };
        private static readonly AsyncCallback SecondAsyncCallback = ar => { Debug.Fail("Complete called more than once."); };

        private readonly ITrace _trace;
        private readonly IDisposable _cleanup;

        private AsyncCallback _callback;
        private volatile bool _isCompleted;

        private Exception _exception;

        internal CallContextAsyncResult(IDisposable cleanup, AsyncCallback callback, object extraData)
        {
            _cleanup = cleanup;
            _callback = callback ?? NoopAsyncCallback;
            AsyncState = extraData;
            _trace = TraceFactory.Create(TraceName);
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                Contract.Assert(false, "Sync APIs and blocking are not supported by the OwinHttpModule");
                // Can't throw, Asp.Net will choke. It will poll IsCompleted instead.
                return null;
            }
        }

        public object AsyncState { get; private set; }

        public bool CompletedSynchronously { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Users callback must not throw")]
        public void Complete(bool completedSynchronously, Exception exception)
        {
            _exception = exception;

            CompletedSynchronously = completedSynchronously;

            _isCompleted = true;
            try
            {
                Interlocked.Exchange(ref _callback, SecondAsyncCallback).Invoke(this);
            }
            catch (Exception ex)
            {
                _trace.WriteError(Resources.Trace_OwinCallContextCallbackException, ex);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "False positive")]
        public static void End(IAsyncResult result)
        {
            var self = result as CallContextAsyncResult;
            if (self == null)
            {
                // "EndProcessRequest must be called with return value of BeginProcessRequest"
                throw new ArgumentException(string.Empty, "result");
            }
            if (self._cleanup != null)
            {
                self._cleanup.Dispose();
            }
            if (self._exception != null)
            {
#if NET40
                Utils.RethrowWithOriginalStack(self._exception);
#else
                // TODO: This is a temporary fix. It preserves the stack trace but not the Watson data.
                // The correct fix is to call Capture in the original catch block and then pass the ExceptionDataInfo
                // instance through. This is impractical while 4.0 is supported.
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(self._exception).Throw();
#endif
            }
            if (!self.IsCompleted)
            {
                // Calling EndProcessRequest before IsComplete is true is not allowed
                throw new ArgumentException(string.Empty, "result");
            }
        }
    }
}
