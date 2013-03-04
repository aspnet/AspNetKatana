// <copyright file="DefaultThreadingServices.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Throttling.Implementation
{
    public class DefaultThreadingServices : IThreadingServices
    {
        public ThreadCounts GetAvailableThreads()
        {
            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            return new ThreadCounts
            {
                WorkerThreads = workerThreads,
                CompletionPortThreads = completionPortThreads
            };
        }

        public ThreadCounts GetMaxThreads()
        {
            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            return new ThreadCounts
            {
                WorkerThreads = workerThreads,
                CompletionPortThreads = completionPortThreads
            };
        }

        public void QueueCallback(WaitCallback callback, object state)
        {
            ThreadPool.UnsafeQueueUserWorkItem(callback, state);
        }

        public IDisposable TimerCallback(TimeSpan interval, Action callback)
        {
            var timer = new System.Timers.Timer(interval.TotalMilliseconds);
            timer.Elapsed += (_, __) => callback();
            timer.Start();
            return timer;
        }
    }
}
