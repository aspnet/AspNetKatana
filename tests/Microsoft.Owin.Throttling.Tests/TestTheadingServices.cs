using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Microsoft.Owin.Throttling.Implementation;

namespace Microsoft.Owin.Throttling.Tests
{
    public class TestTheadingServices : IThreadingServices
    {
        public TestTheadingServices()
        {
            MaxThreads = new ThreadCounts(short.MaxValue, 1000);
            MinThreads = new ThreadCounts(8, 4);
            AvailableThreads = MaxThreads;
            Callbacks = new List<Tuple<WaitCallback, object>>();
            Timers = new List<Tuple<TimeSpan, Action>>();
        }

        public ThreadCounts MaxThreads { get; set; }
        public ThreadCounts MinThreads { get; set; }
        public ThreadCounts AvailableThreads { get; set; }

        public List<Tuple<WaitCallback, object>> Callbacks { get; set; }
        public List<Tuple<TimeSpan, Action>> Timers { get; set; }

        public ThreadCounts GetMaxThreads()
        {
            return MaxThreads;
        }

        public ThreadCounts GetMinThreads()
        {
            return MinThreads;
        }

        public ThreadCounts GetAvailableThreads()
        {
            return AvailableThreads;
        }

        public void QueueCallback(WaitCallback callback, object state)
        {
            Callbacks.Add(Tuple.Create(callback, state));
        }

        public IDisposable TimerCallback(TimeSpan interval, Action callback)
        {
            Timers.Add(Tuple.Create(interval, callback));
            return null;
        }

        public void DoOneCallback()
        {
            var cb = Callbacks[0];
            Callbacks.RemoveAt(0);
            cb.Item1.Invoke(cb.Item2);
        }
    }
}