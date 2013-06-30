// <copyright file="RequestQueueTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading.Tasks;
using Microsoft.Owin.Throttling.Implementation;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Throttling.Tests
{
    public class RequestQueueTests
    {
        private readonly Func<IDictionary<string, object>, Task> _app;
        private readonly TestTheadingServices _threading;
        private readonly ThrottlingOptions _options;
        private RequestQueue _queue;

        public RequestQueueTests()
        {
            _threading = new TestTheadingServices();
            _options = new ThrottlingOptions
            {
                ThreadingServices = _threading
            };

            CreateRequestQueue();

            _app = env => Task.FromResult(0);
        }

        private void CreateRequestQueue()
        {
            _queue = new RequestQueue(_options);
            _queue.Start();
        }

        private RequestInstance BuildRequest(Action<OwinRequest> configure)
        {
            OwinRequest request = new OwinRequest();
            configure(request);
            return new RequestInstance(request.Environment, _app);
        }

        [Fact]
        public void SameContextComesBackNormally()
        {
            RequestInstance requestInstance = BuildRequest(r => { });
            RequestInstance executeInstance = _queue.GetInstanceToExecute(requestInstance);
            requestInstance.ShouldBeSameAs(executeInstance);
        }

        [Fact]
        public void NullContextComesBackWhenEitherPoolExceeds()
        {
            RequestInstance requestInstance = BuildRequest(r => { });

            _threading.AvailableThreads = new ThreadCounts(0, _threading.MaxThreads.CompletionPortThreads);
            _queue.GetInstanceToExecute(requestInstance).ShouldBe(null);

            _threading.AvailableThreads = new ThreadCounts(_threading.MaxThreads.WorkerThreads, 0);
            _queue.GetInstanceToExecute(requestInstance).ShouldBe(null);

            _threading.AvailableThreads = _threading.MaxThreads;
            _queue.GetInstanceToExecute(requestInstance).ShouldNotBe(null);
        }

        [Fact]
        public void EarlierInstanceComesBackIfItWasQueued()
        {
            RequestInstance requestInstance1 = BuildRequest(r => { });
            RequestInstance requestInstance2 = BuildRequest(r => { });

            _threading.AvailableThreads = ThreadCounts.Zero;
            _queue.GetInstanceToExecute(requestInstance1).ShouldBe(null);

            _threading.AvailableThreads = _threading.MaxThreads;
            _queue.GetInstanceToExecute(requestInstance2).ShouldBe(requestInstance1);
        }

        [Fact]
        public void LocalInstanceDequeuesFirst()
        {
            RequestInstance requestInstance1 = BuildRequest(r => r.Set("server.IsLocal", false));
            RequestInstance requestInstance2 = BuildRequest(r => r.Set("server.IsLocal", true));
            RequestInstance requestInstance3 = BuildRequest(r => { });
            RequestInstance requestInstance4 = BuildRequest(r => { });

            _threading.AvailableThreads = ThreadCounts.Zero;
            _queue.GetInstanceToExecute(requestInstance1).ShouldBe(null);
            _queue.GetInstanceToExecute(requestInstance2).ShouldBe(null);

            _threading.AvailableThreads = _threading.MaxThreads;
            _queue.GetInstanceToExecute(requestInstance3).ShouldBeSameAs(requestInstance2);
            _queue.GetInstanceToExecute(requestInstance4).ShouldBeSameAs(requestInstance1);
        }

        [Fact]
        public void RequestsRejectWhenQueueTooLong()
        {
            _options.QueueLengthBeforeIncomingRequestsRejected = 2;
            CreateRequestQueue();

            _threading.AvailableThreads = ThreadCounts.Zero;

            RequestInstance requestInstance1 = BuildRequest(r => { });
            RequestInstance requestInstance2 = BuildRequest(r => { });
            RequestInstance requestInstance3 = BuildRequest(r => { });

            _queue.GetInstanceToExecute(requestInstance1).ShouldBe(null);
            _queue.GetInstanceToExecute(requestInstance2).ShouldBe(null);
            _queue.GetInstanceToExecute(requestInstance3).ShouldBe(null);

            requestInstance1.Task.IsCompleted.ShouldBe(false);
            requestInstance2.Task.IsCompleted.ShouldBe(false);
            requestInstance3.Task.IsCompleted.ShouldBe(true);
        }

        [Fact]
        public void OnlyLocalRequestsExecuteAtCertainLevels()
        {
            int halfway = (_options.ActiveThreadsBeforeRemoteRequestsQueue + _options.ActiveThreadsBeforeLocalRequestsQueue) / 2;

            _threading.AvailableThreads = _threading.MaxThreads.Subtract(new ThreadCounts(halfway, halfway));

            RequestInstance requestInstance1 = BuildRequest(r => r.Set("server.IsLocal", false));
            RequestInstance requestInstance2 = BuildRequest(r => r.Set("server.IsLocal", true));
            RequestInstance requestInstance3 = BuildRequest(r => r.Set("server.IsLocal", false));

            _queue.GetInstanceToExecute(requestInstance1).ShouldBe(null);
            _queue.GetInstanceToExecute(requestInstance2).ShouldBe(requestInstance2);
            _queue.GetInstanceToExecute(requestInstance3).ShouldBe(null);
        }

        [Fact]
        public void OnlyTwoCallbacksWillBeScheduledWhenIoThreadsAreBusy()
        {
            _threading.AvailableThreads = new ThreadCounts(_threading.MaxThreads.WorkerThreads, 0);

            _threading.Callbacks.Count.ShouldBe(0);
            _queue.GetInstanceToExecute(BuildRequest(r => { })).ShouldBe(null);
            _threading.Callbacks.Count.ShouldBe(1);
            _queue.GetInstanceToExecute(BuildRequest(r => { })).ShouldBe(null);
            _threading.Callbacks.Count.ShouldBe(2);
            _queue.GetInstanceToExecute(BuildRequest(r => { })).ShouldBe(null);
            _threading.Callbacks.Count.ShouldBe(2);
            _queue.GetInstanceToExecute(BuildRequest(r => { })).ShouldBe(null);
            _threading.Callbacks.Count.ShouldBe(2);
        }

        [Fact]
        public void CallbacksWillExecuteRequestsInOrder()
        {
            _threading.AvailableThreads = new ThreadCounts(_threading.MaxThreads.WorkerThreads, 0);

            RequestInstance req1 = BuildRequest(r => { });
            RequestInstance req2 = BuildRequest(r => { });
            RequestInstance req3 = BuildRequest(r => { });
            RequestInstance req4 = BuildRequest(r => { });

            _queue.GetInstanceToExecute(req1).ShouldBe(null);
            _queue.GetInstanceToExecute(req2).ShouldBe(null);
            _queue.GetInstanceToExecute(req3).ShouldBe(null);
            _queue.GetInstanceToExecute(req4).ShouldBe(null);

            _threading.Callbacks.Count.ShouldBe(2);
            req1.Task.IsCompleted.ShouldBe(false);
            req2.Task.IsCompleted.ShouldBe(false);
            req3.Task.IsCompleted.ShouldBe(false);
            req4.Task.IsCompleted.ShouldBe(false);

            _threading.AvailableThreads = _threading.MaxThreads;

            _threading.DoOneCallback();
            _threading.Callbacks.Count.ShouldBe(2);
            req1.Task.IsCompleted.ShouldBe(true);
            req2.Task.IsCompleted.ShouldBe(false);
            req3.Task.IsCompleted.ShouldBe(false);
            req4.Task.IsCompleted.ShouldBe(false);

            _threading.DoOneCallback();
            _threading.Callbacks.Count.ShouldBe(2);
            req1.Task.IsCompleted.ShouldBe(true);
            req2.Task.IsCompleted.ShouldBe(true);
            req3.Task.IsCompleted.ShouldBe(false);
            req4.Task.IsCompleted.ShouldBe(false);

            _threading.DoOneCallback();
            _threading.Callbacks.Count.ShouldBe(2);
            req1.Task.IsCompleted.ShouldBe(true);
            req2.Task.IsCompleted.ShouldBe(true);
            req3.Task.IsCompleted.ShouldBe(true);
            req4.Task.IsCompleted.ShouldBe(false);

            _threading.DoOneCallback();
            _threading.Callbacks.Count.ShouldBe(1);
            req1.Task.IsCompleted.ShouldBe(true);
            req2.Task.IsCompleted.ShouldBe(true);
            req3.Task.IsCompleted.ShouldBe(true);
            req4.Task.IsCompleted.ShouldBe(true);

            _threading.DoOneCallback();
            _threading.Callbacks.Count.ShouldBe(0);
            req1.Task.IsCompleted.ShouldBe(true);
            req2.Task.IsCompleted.ShouldBe(true);
            req3.Task.IsCompleted.ShouldBe(true);
            req4.Task.IsCompleted.ShouldBe(true);
        }
    }
}
