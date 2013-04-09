// <copyright file="IntegratedPipelineContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.IntegratedPipeline
{
    internal class IntegratedPipelineContext
    {
        private readonly IntegratedPipelineBlueprint _blueprint;

        private State _state;

        public IntegratedPipelineContext(IntegratedPipelineBlueprint blueprint)
        {
            _blueprint = blueprint;
        }

        public bool PreventNextStage
        {
            get { return _state.PreventNextStage; }
            set { _state.PreventNextStage = value; }
        }

        public void Initialize(HttpApplication application)
        {
            for (IntegratedPipelineBlueprintStage stage = _blueprint.FirstStage; stage != null; stage = stage.NextStage)
            {
                var segment = new IntegratedPipelineContextStage(this, stage);
                switch (stage.Name)
                {
                    case "Authenticate":
                        application.AddOnAuthenticateRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "PostAuthenticate":
                        application.AddOnPostAuthenticateRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "Authorize":
                        application.AddOnAuthorizeRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "PostAuthorize":
                        application.AddOnPostAuthorizeRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "ResolveCache":
                        application.AddOnResolveRequestCacheAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "PostResolveCache":
                        application.AddOnPostResolveRequestCacheAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "MapHandler":
                        application.AddOnMapRequestHandlerAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "PostMapHandler":
                        application.AddOnPostMapRequestHandlerAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "AcquireState":
                        application.AddOnAcquireRequestStateAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "PostAcquireState":
                        application.AddOnPostAcquireRequestStateAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case "PreHandlerExecute":
                        application.AddOnPreRequestHandlerExecuteAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            application.AddOnEndRequestAsync(BeginFinalWork, EndFinalWork);
        }

        private void Reset()
        {
            PushExecutingStage(null);
            _state = new State();
        }

        public static Task DefaultAppInvoked(IDictionary<string, object> env)
        {
            object value;
            if (env.TryGetValue("integratedpipeline.Context", out value))
            {
                var self = (IntegratedPipelineContext)value;
                return self._state.ExecutingStage.DefaultAppInvoked(env);
            }
            throw new InvalidOperationException();
        }

        public static Task ExitPointInvoked(IDictionary<string, object> env)
        {
            object value;
            if (env.TryGetValue("integratedpipeline.Context", out value))
            {
                var self = (IntegratedPipelineContext)value;
                return self._state.ExecutingStage.ExitPointInvoked(env);
            }
            throw new InvalidOperationException();
        }

        private IAsyncResult BeginFinalWork(object sender, EventArgs e, AsyncCallback cb, object extradata)
        {
            var result = new StageAsyncResult(cb, extradata, () => { });
            TaskCompletionSource<object> tcs = TakeLastCompletionSource();
            if (tcs != null)
            {
                tcs.TrySetResult(null);
            }
            if (_state.OriginalTask != null)
            {
                _state.OriginalTask
                    .Then(() => _state.CallContext.OnEnd())
                    .Finally(result.TryComplete);
            }
            else
            {
                result.TryComplete();
            }
            result.InitialThreadReturning();
            return result;
        }

        private void EndFinalWork(IAsyncResult ar)
        {
            Reset();
            StageAsyncResult.End(ar);
        }

        public Func<IDictionary<string, object>, Task> PrepareInitialContext(HttpApplication application)
        {
            IDictionary<string, object> environment = GetInitialEnvironment(application);
            var originalCompletionSource = new TaskCompletionSource<object>();
            _state.OriginalTask = originalCompletionSource.Task;
            PushLastObjects(environment, originalCompletionSource);
            return _blueprint.AppContext.AppFunc;
        }

        public IDictionary<string, object> GetInitialEnvironment(HttpApplication application)
        {
            if (_state.CallContext != null)
            {
                return _state.CallContext.Environment;
            }

            string requestPath = application.Request.AppRelativeCurrentExecutionFilePath.Substring(1) + application.Request.PathInfo;

            _state.CallContext = _blueprint.AppContext.CreateCallContext(
                application.Request.RequestContext,
                _blueprint.PathBase,
                requestPath,
                null,
                null);

            _state.CallContext.CreateEnvironment();

            IDictionary<string, object> environment = _state.CallContext.Environment;
            environment["integratedpipeline.Context"] = this;
            return environment;
        }

        public void PushExecutingStage(IntegratedPipelineContextStage stage)
        {
            IntegratedPipelineContextStage prior = Interlocked.Exchange(ref _state.ExecutingStage, stage);
            if (prior != null)
            {
                prior.Reset();
            }
        }

        public void PushLastObjects(IDictionary<string, object> environment, TaskCompletionSource<object> completionSource)
        {
            IDictionary<string, object> priorEnvironment = Interlocked.CompareExchange(ref _state.LastEnvironment, environment, null);
            TaskCompletionSource<object> priorCompletionSource = Interlocked.CompareExchange(ref _state.LastCompletionSource, completionSource, null);

            if (priorEnvironment != null || priorCompletionSource != null)
            {
                // TODO: trace fatal condition
                throw new InvalidOperationException();
            }
        }

        public IDictionary<string, object> TakeLastEnvironment()
        {
            return Interlocked.Exchange(ref _state.LastEnvironment, null);
        }

        public TaskCompletionSource<object> TakeLastCompletionSource()
        {
            return Interlocked.Exchange(ref _state.LastCompletionSource, null);
        }

        private struct State
        {
            public IDictionary<string, object> LastEnvironment;
            public TaskCompletionSource<object> LastCompletionSource;
            public Task OriginalTask;
            public OwinCallContext CallContext;
            public bool PreventNextStage;
            public IntegratedPipelineContextStage ExecutingStage;
        }
    }
}
