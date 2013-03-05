// <copyright file="TestTheadingServices.cs" company="Microsoft Open Technologies, Inc.">
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
            Tuple<WaitCallback, object> cb = Callbacks[0];
            Callbacks.RemoveAt(0);
            cb.Item1.Invoke(cb.Item2);
        }
    }
}
