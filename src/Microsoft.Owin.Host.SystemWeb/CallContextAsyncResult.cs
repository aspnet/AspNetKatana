// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        private static readonly ITrace Trace = TraceFactory.Create(TraceName);

        private readonly IDisposable _cleanup;

        private AsyncCallback _callback;
        private volatile bool _isCompleted;

        private ErrorState _errorState;

        internal CallContextAsyncResult(IDisposable cleanup, AsyncCallback callback, object extraData)
        {
            _cleanup = cleanup;
            _callback = callback ?? NoopAsyncCallback;
            AsyncState = extraData;
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
        public void Complete(bool completedSynchronously, ErrorState errorState)
        {
            _errorState = errorState;

            CompletedSynchronously = completedSynchronously;

            _isCompleted = true;
            try
            {
                Interlocked.Exchange(ref _callback, SecondAsyncCallback).Invoke(this);
            }
            catch (Exception ex)
            {
                Trace.WriteError(Resources.Trace_OwinCallContextCallbackException, ex);
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
            if (self._errorState != null)
            {
                self._errorState.Rethrow();
            }
            if (!self.IsCompleted)
            {
                // Calling EndProcessRequest before IsComplete is true is not allowed
                throw new ArgumentException(string.Empty, "result");
            }
        }
    }
}
