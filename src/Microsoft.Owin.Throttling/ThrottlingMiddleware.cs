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
        private readonly RequestQueue _queue;

        public ThrottlingMiddleware(AppFunc next, ThrottlingOptions options)
        {
            _next = next;
            _options = options;
            _queue = new RequestQueue(_options);
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var requestContext = new RequestInstance(env);
            var executeContext = _queue.GetInstanceToExecute(requestContext);
            if (executeContext != null)
            {
                executeContext.Invoke(_next);
            }
            return requestContext.Task;
        }
    }

    public class RequestInstance
    {
        private static readonly Task CompletedTask = MakeCompletedTask();
        private readonly IDictionary<string, object> _env;
        private Task _task;
        private TaskCompletionSource<object> _tcs;
        private ExecutionContext _executionContext;

        public RequestInstance(IDictionary<string, object> env)
        {
            _env = env;
        }

        public Task Task
        {
            get { return _task; }
        }

        public bool IsLocal
        {
            get
            {
                object value;
                return _env.TryGetValue("server.IsLocal", out value) && (bool)value;
            }
        }

        public bool IsConnected
        {
            get { return true; }
        }

        public void Defer()
        {
            _executionContext = ExecutionContext.Capture();
            _tcs = new TaskCompletionSource<object>();
            _task = _tcs.Task;
        }

        public void Invoke(AppFunc next)
        {
            if (_tcs == null)
            {
                _task = next(_env);
            }
            else
            {
                ExecutionContext.Run(
                    _executionContext,
                    InvokeCallback,
                    next);
            }
        }

        public void InvokeCallback(object state)
        {
            try
            {
                var next = (AppFunc)state;
                var task = next(_env);
                if (task.IsCompleted)
                {
                    if (task.IsFaulted)
                    {
                        _tcs.TrySetException(task.Exception);
                    }
                    else if (task.IsCanceled)
                    {
                        _tcs.TrySetCanceled();
                    }
                    else
                    {
                        _tcs.TrySetResult(null);
                    }
                }
                else
                {
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            _tcs.TrySetException(t.Exception);
                        }
                        else if (t.IsCanceled)
                        {
                            _tcs.TrySetCanceled();
                        }
                        else
                        {
                            _tcs.TrySetResult(null);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
        }

        public void Reject()
        {
            _env["owin.StatusCode"] = 503;
            _task = CompletedTask;
        }

        private static Task MakeCompletedTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}
