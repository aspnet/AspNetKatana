// <copyright file="StageAsyncResult.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading;

namespace Microsoft.Owin.Host.SystemWeb.IntegratedPipeline
{
    internal class StageAsyncResult : IAsyncResult
    {
        private readonly AsyncCallback _callback;
        private readonly Action _completing;
        private volatile int _managedThreadId;
        private int _completions;

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
        }
    }
}
