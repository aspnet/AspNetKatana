// <copyright file="IntegratedPipelineContextStage.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.IntegratedPipeline
{
    internal class IntegratedPipelineContextStage
    {
        private readonly IntegratedPipelineContext _context;
        private readonly IntegratedPipelineBlueprintStage _stage;
        private StageAsyncResult _result;
        private bool _responseShouldEnd;

        public IntegratedPipelineContextStage(IntegratedPipelineContext context, IntegratedPipelineBlueprintStage stage)
        {
            _context = context;
            _stage = stage;
        }

        public void Reset()
        {
            _result = null;
            _responseShouldEnd = false;
        }

        public IAsyncResult BeginEvent(object sender, EventArgs e, AsyncCallback cb, object extradata)
        {
            if (_result != null)
            {
                throw new InvalidOperationException();
            }

            if (_context.PreventNextStage)
            {
                var noop = new StageAsyncResult(cb, extradata, () => { });
                noop.TryComplete();
                noop.InitialThreadReturning();
                return noop;
            }

            _context.PreventNextStage = true;
            _responseShouldEnd = true;
            _context.PushExecutingStage(this);

            Func<IDictionary<string, object>, Task> entryPoint = _stage.EntryPoint ?? _context.PrepareInitialContext((HttpApplication)sender);
            IDictionary<string, object> environment = _context.TakeLastEnvironment();
            TaskCompletionSource<object> tcs = _context.TakeLastCompletionSource();

            var result = new StageAsyncResult(cb, extradata, () =>
            {
                var application = ((HttpApplication)sender);

                // TODO: modify via assignment instead
                IPrincipal contextUser = application.Context.User;
                var owinUser = (IPrincipal)environment["server.User"];
                if (contextUser != owinUser && owinUser != null)
                {
                    application.Context.User = owinUser;
                }

                if (_responseShouldEnd)
                {
                    application.CompleteRequest();
                }
            });

            _result = result;
            Task task1 = entryPoint.Invoke(environment);
            Task task2 = task1.CopyResultToCompletionSource(tcs, null);
            Task task3 = task2.ContinueWith(t => result.TryComplete(), TaskContinuationOptions.ExecuteSynchronously);
            task3.Catch(ci => ci.Handled());
            result.InitialThreadReturning();
            return result;
        }

        public void EndEvent(IAsyncResult ar)
        {
            StageAsyncResult.End(ar);
        }

        public Task DefaultAppInvoked(IDictionary<string, object> env)
        {
            return Epilog(env);
        }

        public Task ExitPointInvoked(IDictionary<string, object> env)
        {
            _context.PreventNextStage = false;
            return Epilog(env);
        }

        private Task Epilog(IDictionary<string, object> env)
        {
            var tcs = new TaskCompletionSource<object>();
            _responseShouldEnd = false;
            _context.PushLastObjects(env, tcs);
            StageAsyncResult result = Interlocked.Exchange(ref _result, null);
            if (result != null)
            {
                result.TryComplete();
            }
            return tcs.Task;
        }
    }
}
