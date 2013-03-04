using System;
using System.Threading;

namespace Microsoft.Owin.Throttling.Implementation
{
    public interface IThreadingServices
    {
        ThreadCounts GetAvailableThreads();
        ThreadCounts GetMaxThreads();
        void QueueCallback(WaitCallback callback, object state);
        IDisposable TimerCallback(TimeSpan interval, Action callback);
    }
}