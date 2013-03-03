using System.Collections.Generic;

namespace Microsoft.Owin.Throttling.Implementation
{
    public class RequestQueue
    {
        private readonly ThrottlingOptions _options;
        private readonly IThreadingServices _threading;
        private ThreadCounts _maxThreads;
        private int _count;
        private readonly Queue<RequestInstance> _remote = new Queue<RequestInstance>();
        private readonly Queue<RequestInstance> _local = new Queue<RequestInstance>();
        private readonly object _sync = new object();

        public RequestQueue(ThrottlingOptions options)
        {
            _options = options;
            _threading = _options.ThreadingServices;
            _maxThreads = _threading.GetMaxThreads();
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

            ScheduleMoreWorkIfNeeded();
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

        private void ScheduleMoreWorkIfNeeded()
        {
        }
    }
}