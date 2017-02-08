// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb.IntegratedPipeline
{
    internal class StageAsyncResult : IAsyncResult
    {
        private readonly AsyncCallback _callback;
        private readonly Action _completing;
        private volatile int _managedThreadId;
        private int _completions;
        private ErrorState _error;

        public StageAsyncResult(AsyncCallback callback, object extradata, Action completing)
        {
            _managedThreadId = Thread.CurrentThread.ManagedThreadId;
            _callback = callback;
            AsyncState = extradata;
            _completing = completing;
        }

        public WaitHandle AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsCompleted { get; private set; }

        public object AsyncState { get; private set; }

        public bool CompletedSynchronously { get; private set; }

        public void InitialThreadReturning()
        {
            _managedThreadId = Int32.MinValue;
        }

        public void Fail(ErrorState error)
        {
            _error = error;
            TryComplete();
        }

        public void TryComplete()
        {
            if (Interlocked.Increment(ref _completions) != 1)
            {
                return;
            }

            if (_managedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                CompletedSynchronously = true;
            }
            IsCompleted = true;
            _completing.Invoke();
            if (_callback != null)
            {
                _callback.Invoke(this);
            }
        }

        public static void End(IAsyncResult ar)
        {
            var result = (StageAsyncResult)ar;
            if (!result.IsCompleted)
            {
                throw new NotImplementedException();
            }
            if (result._error != null)
            {
                result._error.Rethrow();
            }
        }
    }
}
