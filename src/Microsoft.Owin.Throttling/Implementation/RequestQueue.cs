using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Owin.Throttling.Implementation
{
    public class RequestQueue
    {
        private readonly ThrottlingOptions _options;
        private readonly IThreadingServices _threading;
        private ThreadCounts _maxThreads;
        private int _count;
        private bool _stopping;
        private readonly Queue<RequestInstance> _remote = new Queue<RequestInstance>();
        private readonly Queue<RequestInstance> _local = new Queue<RequestInstance>();
        private readonly object _sync = new object();
        private int _scheduled;
        private readonly WaitCallback _executeIfNeeded;
        private IDisposable _timer;

        public RequestQueue(ThrottlingOptions options)
        {
            _options = options;
            _threading = _options.ThreadingServices;
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
            _timer.Dispose();
        }

        public RequestInstance GetInstanceToExecute(RequestInstance requestInstance)
        {
            var availableCounts = _threading.GetAvailableThreads();
            var activeCounts = _maxThreads.Subtract(availableCounts);
            var active = activeCounts.Greatest();
            if (_count == 0 && active < _options.ActiveThreadsPerCpuBeforeRemoteRequestsQueue)
            {
                return requestInstance;
            }

            var isLocal = requestInstance.IsLocal;
            if (_count >= _options.RequestQueueLimitBeforeServerTooBusyResponse)
            {
                RejectInstance(requestInstance, false);
                return null;
            }

            requestInstance.Defer();
            QueueInstance(requestInstance, isLocal);
            if (active < _options.ActiveThreadsPerCpuBeforeRemoteRequestsQueue)
            {
                return DequeueInstance(false);
            }

            if (active < _options.ActiveThreadsPerCpuBeforeLocalRequestsQueue)
            {
                return DequeueInstance(true);
            }

            ScheduleExecuteIfNeeded();
            return null;
        }

        private void RejectInstance(RequestInstance instance, bool silent)
        {
            if (silent)
            {
                return;
            }
            instance.Reject();
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
                RejectInstance(instance, true);
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
            var available = _threading.GetAvailableThreads();
            var active = _maxThreads.WorkerThreads - available.WorkerThreads;
            if (active >= _options.ActiveThreadsPerCpuBeforeLocalRequestsQueue)
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
            var available = _threading.GetAvailableThreads();
            var active = _maxThreads.WorkerThreads - available.WorkerThreads;
            if (active >= _options.ActiveThreadsPerCpuBeforeLocalRequestsQueue)
            {
                return;
            }
            var localOnly = active >= _options.ActiveThreadsPerCpuBeforeRemoteRequestsQueue;
            var instance = DequeueInstance(localOnly);
            if (instance == null)
            {
                return;
            }
            ScheduleExecuteIfNeeded();
            instance.Execute();
        }
    }
}