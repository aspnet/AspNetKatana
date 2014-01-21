// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb.IntegratedPipeline
{
    internal class IntegratedPipelineContext
    {
        // Ordered list of supported stage names
        private static readonly IList<string> StageNames = new[]
        {
            Constants.StageAuthenticate,
            Constants.StagePostAuthenticate,
            Constants.StageAuthorize,
            Constants.StagePostAuthorize,
            Constants.StageResolveCache,
            Constants.StagePostResolveCache,
            Constants.StageMapHandler,
            Constants.StagePostMapHandler,
            Constants.StageAcquireState,
            Constants.StagePostAcquireState,
            Constants.StagePreHandlerExecute,
        };

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
                    case Constants.StageAuthenticate:
                        application.AddOnAuthenticateRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StagePostAuthenticate:
                        application.AddOnPostAuthenticateRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StageAuthorize:
                        application.AddOnAuthorizeRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StagePostAuthorize:
                        application.AddOnPostAuthorizeRequestAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StageResolveCache:
                        application.AddOnResolveRequestCacheAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StagePostResolveCache:
                        application.AddOnPostResolveRequestCacheAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StageMapHandler:
                        application.AddOnMapRequestHandlerAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StagePostMapHandler:
                        application.AddOnPostMapRequestHandlerAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StageAcquireState:
                        application.AddOnAcquireRequestStateAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StagePostAcquireState:
                        application.AddOnPostAcquireRequestStateAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    case Constants.StagePreHandlerExecute:
                        application.AddOnPreRequestHandlerExecuteAsync(segment.BeginEvent, segment.EndEvent);
                        break;
                    default:
                        throw new NotSupportedException(
                            string.Format(CultureInfo.InvariantCulture, Resources.Exception_UnsupportedPipelineStage, stage.Name));
                }
            }
            // application.PreSendRequestHeaders += PreSendRequestHeaders; // Null refs for async un-buffered requests with bodies.
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
            if (env.TryGetValue(Constants.IntegratedPipelineContext, out value))
            {
                var self = (IntegratedPipelineContext)value;
                return self._state.ExecutingStage.DefaultAppInvoked(env);
            }
            throw new InvalidOperationException();
        }

        public static Task ExitPointInvoked(IDictionary<string, object> env)
        {
            object value;
            if (env.TryGetValue(Constants.IntegratedPipelineContext, out value))
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
                DoFinalWork(result);
            }
            else
            {
                result.TryComplete();
            }
            result.InitialThreadReturning();
            return result;
        }

        private async void DoFinalWork(StageAsyncResult result)
        {
            try
            {
                await _state.OriginalTask;
                _state.CallContext.OnEnd();
                CallContextAsyncResult.End(_state.CallContext.AsyncResult);
                result.TryComplete();
            }
            catch (Exception ex)
            {
                _state.CallContext.AbortIfHeaderSent();
                result.Fail(ErrorState.Capture(ex));
            }
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

            AspNetDictionary environment = _state.CallContext.Environment;
            environment.IntegratedPipelineContext = this;
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

        // Does stage1 come before stage2?
        // Returns false for unknown stages, or equal stages.
        internal static bool VerifyStageOrder(string stage1, string stage2)
        {
            int stage1Index = StageNames.IndexOf(stage1);
            int stage2Index = StageNames.IndexOf(stage2);

            if (stage1Index == -1 || stage2Index == -1)
            {
                return false;
            }
            return stage1Index < stage2Index;
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
