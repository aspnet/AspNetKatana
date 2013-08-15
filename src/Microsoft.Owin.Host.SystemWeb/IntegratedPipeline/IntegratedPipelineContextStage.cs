// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Directing to callback")]
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

                if (_responseShouldEnd)
                {
                    application.CompleteRequest();
                }
            });

            _result = result;

            environment[Constants.IntegratedPipelineCurrentStage] = _stage.Name;

            try
            {
                entryPoint.Invoke(environment)
                          .CopyResultToCompletionSource(tcs, null)
                          .ContinueWith(t => result.TryComplete(), TaskContinuationOptions.ExecuteSynchronously)
                          .Catch(ci => ci.Handled());
            }
            catch (Exception ex)
            {
                // Flow the exception back through the OWIN pipeline.
                tcs.TrySetException(ex);
                result.TryComplete();
                return result;
            }

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
