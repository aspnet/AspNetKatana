// <copyright file="ThrottlingMiddleware.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading.Tasks;
using Microsoft.Owin.Throttling.Implementation;

namespace Microsoft.Owin.Throttling
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ThrottlingMiddleware
    {
        private readonly AppFunc _next;
        private readonly ThrottlingOptions _options;
        private RequestQueue _queue;
        private bool _queueInitialized;
        private object _queueLock = new object();

        public ThrottlingMiddleware(AppFunc next, ThrottlingOptions options)
        {
            _next = next;
            _options = options;
        }

        private bool IsQueueInitialized
        {
            get
            {
                bool value = _queueInitialized;
                Thread.MemoryBarrier();
                return value;
            }
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            RequestQueue queue;
            if (IsQueueInitialized)
            {
                queue = _queue;
            }
            else
            {
                queue = LazyInitializer.EnsureInitialized(
                    ref _queue,
                    ref _queueInitialized,
                    ref _queueLock,
                    () =>
                    {
                        // Start called once on first request
                        var newQueue = new RequestQueue(_options);
                        newQueue.Start();
                        object value;
                        CancellationToken onAppDisposing = env.TryGetValue("server.OnAppDisposing", out value) ? (CancellationToken)value : CancellationToken.None;
                        if (onAppDisposing != CancellationToken.None)
                        {
                            // Stop called once on app disposing
                            // will drain queue by rejecting
                            onAppDisposing.Register(newQueue.Stop);
                        }
                        return newQueue;
                    });
            }

            var requestInstance = new RequestInstance(env, _next);
            RequestInstance executeInstance = queue.GetInstanceToExecute(requestInstance);
            if (executeInstance != null)
            {
                executeInstance.Execute();
            }
            return requestInstance.Task;
        }
    }
}
