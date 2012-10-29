// <copyright file="OwinCallContext.IAsyncResult.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Host.SystemWeb
{
    public partial class OwinCallContext : IAsyncResult
    {
        private static readonly AsyncCallback NoopAsyncCallback =
            ar => { };

        private static readonly AsyncCallback ExtraAsyncCallback =
            ar => Trace.WriteLine("OwinHttpHandler: more than one call to complete the same AsyncResult");

        private readonly TaskCompletionSource<Nada> _taskCompletionSource = new TaskCompletionSource<Nada>();

        private AsyncCallback _cb;
        private Exception _exception;

        public OwinCallContext(AsyncCallback cb, object extraData)
        {
            _cb = cb ?? NoopAsyncCallback;
            AsyncState = extraData;
        }

        public bool IsCompleted { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get { throw new InvalidOperationException("Entering a blocking wait state is not allowed"); }
        }

        public object AsyncState { get; private set; }

        public bool CompletedSynchronously { get; private set; }

        public void Complete(bool completedSynchronously, Exception exception)
        {
            _exception = exception;

            if (exception == null)
            {
                _taskCompletionSource.TrySetResult(default(Nada));
            }
            else
            {
                _taskCompletionSource.TrySetException(exception);
            }

            // note: completed synchronously is not set because aspnet execution of begin/end is
            // confused by synchronous completion in the callback iasyncresult 
            // CompletedSynchronously = completedSynchronously; 

            IsCompleted = true;
            try
            {
                Interlocked.Exchange(ref _cb, ExtraAsyncCallback).Invoke(this);
            }
            catch (Exception ex)
            {
                // TODO: certain exception must never be caught - find out what those are and
                // rethrow if ex is one of them
                Trace.WriteLine("OwinHttpHandler: AsyncResult callback threw an exception. " + ex.Message);
            }
        }

        public static void End(IAsyncResult result)
        {
            if (!(result is OwinCallContext))
            {
                throw new InvalidOperationException("EndProcessRequest must be called with return value of BeginProcessRequest");
            }
            var self = ((OwinCallContext)result);
            if (self._exception != null)
            {
                throw new TargetInvocationException(self._exception);
            }
            if (!self.IsCompleted)
            {
                throw new InvalidOperationException("Calling EndProcessRequest before IsComplete is true is not allowed");
            }
        }

        private struct Nada
        {
        }
    }
}
