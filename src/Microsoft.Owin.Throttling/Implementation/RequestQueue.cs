// <copyright file="RequestQueue.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading;

namespace Microsoft.Owin.Throttling.Implementation
{
    public class RequestQueue
    {
        private readonly IThreadingServices _threading;
        private readonly ThreadCounts _maxThreads;
        private readonly Queue<RequestInstance> _remote = new Queue<RequestInstance>();
        private readonly Queue<RequestInstance> _local = new Queue<RequestInstance>();
        private readonly object _sync = new object();
        private readonly WaitCallback _executeIfNeeded;
        private readonly int _activeThreadsBeforeRemoteRequestsQueue;
        private readonly int _activeThreadsBeforeLocalRequestsQueue;
        private readonly int _queueLengthBeforeIncomingRequestsRejected;

        private int _count;
        private bool _stopping;
        private int _scheduled;
        private IDisposable _timer;

        public RequestQueue(ThrottlingOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            _threading = options.ThreadingServices;
            _activeThreadsBeforeLocalRequestsQueue = options.ActiveThreadsBeforeLocalRequestsQueue;
            _activeThreadsBeforeRemoteRequestsQueue = options.ActiveThreadsBeforeRemoteRequestsQueue;
            _queueLengthBeforeIncomingRequestsRejected = options.QueueLengthBeforeIncomingRequestsRejected;

            _maxThreads = _threading.GetMaxThreads();

            _executeIfNeeded = ExecuteIfNeeded;
        }

        public void Start()
        {
            _timer = _threading.TimerCallback(TimeSpan.FromSeconds(10), ScheduleExecuteIfNeeded);
        }

        public void Stop()
        {
            _stopping = true;
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            while (_scheduled > 0)
            {
                Thread.Sleep(100);
            }
            if (_count != 0)
            {
                while (true)
                {
                    RequestInstance instance = DequeueInstance(false);
                    if (instance == null)
                    {
                        break;
                    }
                    instance.Reject();
                }
            }
        }

        public RequestInstance GetInstanceToExecute(RequestInstance requestInstance)
        {
            ThreadCounts availableCounts = _threading.GetAvailableThreads();
            ThreadCounts activeCounts = _maxThreads.Subtract(availableCounts);
            int active = activeCounts.Greatest();
            if (_count == 0 && active < _activeThreadsBeforeRemoteRequestsQueue)
            {
                return requestInstance;
            }

            bool isLocal = requestInstance.IsLocal;
            if (_count >= _queueLengthBeforeIncomingRequestsRejected)
            {
                requestInstance.Reject();
                return null;
            }

            requestInstance.Defer();
            QueueInstance(requestInstance, isLocal);
            if (active < _activeThreadsBeforeRemoteRequestsQueue)
            {
                return DequeueInstance(false);
            }

            if (active < _activeThreadsBeforeLocalRequestsQueue)
            {
                return DequeueInstance(true);
            }

            ScheduleExecuteIfNeeded();
            return null;
        }

        private void QueueInstance(RequestInstance instance, bool isLocal)
        {
            lock (_sync)
            {
                if (isLocal)
                {
                    _local.Enqueue(instance);
                }
                else
                {
                    _remote.Enqueue(instance);
                }
                _count++;
            }
        }

        private RequestInstance DequeueInstance(bool localOnly)
        {
            while (_count != 0)
            {
                RequestInstance instance;
                lock (_sync)
                {
                    if (_local.Count != 0)
                    {
                        instance = _local.Dequeue();
                        _count--;
                    }
                    else if (!localOnly && _remote.Count != 0)
                    {
                        instance = _remote.Dequeue();
                        _count--;
                    }
                    else
                    {
                        return null;
                    }
                }

                if (instance.IsConnected)
                {
                    return instance;
                }
                instance.RejectSilent();
            }
            return null;
        }

        private void ScheduleExecuteIfNeeded()
        {
            if (_stopping)
            {
                return;
            }
            if (_count == 0)
            {
                return;
            }
            if (_scheduled >= 2)
            {
                return;
            }
            ThreadCounts available = _threading.GetAvailableThreads();
            int active = _maxThreads.WorkerThreads - available.WorkerThreads;
            if (active >= _activeThreadsBeforeLocalRequestsQueue)
            {
                return;
            }
            Interlocked.Increment(ref _scheduled);
            _threading.QueueCallback(_executeIfNeeded, null);
        }

        private void ExecuteIfNeeded(object state)
        {
            Interlocked.Decrement(ref _scheduled);
            if (_stopping)
            {
                return;
            }
            if (_count == 0)
            {
                return;
            }
            ThreadCounts available = _threading.GetAvailableThreads();
            int active = _maxThreads.WorkerThreads - available.WorkerThreads;
            if (active >= _activeThreadsBeforeLocalRequestsQueue)
            {
                return;
            }
            bool localOnly = active >= _activeThreadsBeforeRemoteRequestsQueue;
            RequestInstance instance = DequeueInstance(localOnly);
            if (instance == null)
            {
                return;
            }
            ScheduleExecuteIfNeeded();
            instance.Execute();
        }
    }
}
