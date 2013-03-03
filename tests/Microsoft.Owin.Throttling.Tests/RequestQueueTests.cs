using System;
using Microsoft.Owin.Throttling.Implementation;
using Owin.Types;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Throttling.Tests
{
    public class RequestQueueTests
    {
        private readonly TestTheadingServices _threading;
        private readonly ThrottlingOptions _options;
        private readonly RequestQueue _queue;

        public RequestQueueTests()
        {
            _threading = new TestTheadingServices();
            _options = new ThrottlingOptions
            {
                ThreadingServices = _threading
            };

            _queue = new RequestQueue(_options);
        }

        private static RequestInstance BuildRequest(Action<OwinRequest> configure)
        {
            var request = OwinRequest.Create();
            configure(request);
            return new RequestInstance(request.Dictionary);
        }

        [Fact]
        public void SameContextComesBackNormally()
        {
            var requestInstance = new RequestInstance(null);
            var executeInstance = _queue.GetInstanceToExecute(requestInstance);
            requestInstance.ShouldBeSameAs(executeInstance);
        }

        [Fact]
        public void NullContextComesBackWhenEitherPoolExceeds()
        {
            var requestInstance = new RequestInstance(OwinRequest.Create().Dictionary);

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
            var requestInstance1 = BuildRequest(r => { });
            var requestInstance2 = BuildRequest(r => { });

            _threading.AvailableThreads = ThreadCounts.Zero;
            _queue.GetInstanceToExecute(requestInstance1).ShouldBe(null);

            _threading.AvailableThreads = _threading.MaxThreads;
            _queue.GetInstanceToExecute(requestInstance2).ShouldBe(requestInstance1);
        }

        [Fact]
        public void LocalInstanceDequeuesFirst()
        {
            var requestInstance1 = BuildRequest(r => r.IsLocal = false);
            var requestInstance2 = BuildRequest(r => r.IsLocal = true);
            var requestInstance3 = BuildRequest(r => { });
            var requestInstance4 = BuildRequest(r => { });

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
            _options.RequestQueueLimitBeforeServerTooBusyResponse = 2;
            _threading.AvailableThreads = ThreadCounts.Zero;

            var requestInstance1 = BuildRequest(r => { });
            var requestInstance2 = BuildRequest(r => { });
            var requestInstance3 = BuildRequest(r => { });

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
            var halfway = (_options.ActiveThreadsPerCpuBeforeRemoteRequestsQueue + _options.ActiveThreadsPerCpuBeforeLocalRequestsQueue) / 2;

            _threading.AvailableThreads = _threading.MaxThreads.Subtract(new ThreadCounts(halfway, halfway));

            var requestInstance1 = BuildRequest(r => r.IsLocal = false);
            var requestInstance2 = BuildRequest(r => r.IsLocal = true);
            var requestInstance3 = BuildRequest(r => r.IsLocal = false);

            _queue.GetInstanceToExecute(requestInstance1).ShouldBe(null);
            _queue.GetInstanceToExecute(requestInstance2).ShouldBe(requestInstance2);
            _queue.GetInstanceToExecute(requestInstance3).ShouldBe(null);
        }
    }
}
