using System;
using System.Threading;
using System.Timers;

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