// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
using Microsoft.Owin.Host.SystemWeb.IntegratedPipeline;

namespace Microsoft.Owin.Host.SystemWeb
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
        Justification = "Coupling can be reviewed later.")]
    internal partial class OwinCallContext : IDisposable
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.OwinCallContext";
        private static readonly ITrace Trace = TraceFactory.Create(TraceName);

        private readonly SendingHeadersEvent _sendingHeadersEvent = new SendingHeadersEvent();

        private readonly OwinAppContext _appContext;
        private readonly RequestContext _requestContext;
        private readonly string _requestPathBase;
        private readonly string _requestPath;
        private readonly HttpContextBase _httpContext;
        private readonly HttpRequestBase _httpRequest;
        private readonly HttpResponseBase _httpResponse;
        private int _completedSynchronouslyThreadId;
        private AspNetDictionary _env;
        private DisconnectWatcher _disconnectWatcher;

        private Exception _startException;
        private bool _startCalled;
        private object _startLock = new object();
        private bool _headersSent;

        internal OwinCallContext(
            OwinAppContext appContext,
            RequestContext requestContext,
            string requestPathBase,
            string requestPath,
            AsyncCallback cb,
            object extraData)
        {
            _appContext = appContext;
            _requestContext = requestContext;
            _requestPathBase = requestPathBase;
            _requestPath = requestPath;

            AsyncResult = new CallContextAsyncResult(this, cb, extraData);

            _httpContext = _requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;

            _disconnectWatcher = new DisconnectWatcher(_httpResponse);
        }

        internal AspNetDictionary Environment
        {
            get { return _env; }
        }

        internal CallContextAsyncResult AsyncResult { get; private set; }

        internal void Execute()
        {
            CreateEnvironment();

            _completedSynchronouslyThreadId = Thread.CurrentThread.ManagedThreadId;
            try
            {
                _appContext.AppFunc(_env)
                    // We can't use Then/Catch here because they would re-enter the sync context.
                    // The async callback must be called outside of the sync context.
                           .ContinueWith(appTask =>
                           {
                               if (appTask.IsFaulted)
                               {
                                   if (!TryRelayExceptionToIntegratedPipeline(false, appTask.Exception))
                                   {
                                       Complete(ErrorState.Capture(appTask.Exception));
                                   }
                               }
                               else if (appTask.IsCanceled)
                               {
                                   Exception ex = new TaskCanceledException(appTask);
                                   if (!TryRelayExceptionToIntegratedPipeline(false, ex))
                                   {
                                       Complete(ErrorState.Capture(ex));
                                   }
                               }
                               else
                               {
                                   OnEnd();
                               }
                           });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _completedSynchronouslyThreadId = Int32.MinValue;
            }
        }

        internal bool TryRelayExceptionToIntegratedPipeline(bool sync, Exception ex)
        {
            // Flow errors back through the integrated pipeline owin middleware.
            object obj;
            if (Environment.TryGetValue(Constants.IntegratedPipelineContext, out obj))
            {
                var integratedContext = obj as IntegratedPipelineContext;
                if (integratedContext != null)
                {
                    TaskCompletionSource<object> tcs = integratedContext.TakeLastCompletionSource();
                    tcs.TrySetException(ex);
                    AsyncResult.Complete(sync, null);
                    return true;
                }
            }
            return false;
        }

        private X509Certificate LoadClientCert()
        {
            if (_httpContext.Request.IsSecureConnection)
            {
                try
                {
                    if (_httpContext.Request.ClientCertificate != null
                        && _httpContext.Request.ClientCertificate.IsPresent)
                    {
                        return new X509Certificate2(_httpContext.Request.ClientCertificate.Certificate);
                    }
                }
                catch (CryptographicException ce)
                {
                    Trace.WriteError(Resources.Trace_ClientCertException, ce);
                }
            }
            return null;
        }

        private Task LoadClientCertAsync()
        {
            try
            {
                if (_httpContext.Request.ClientCertificate != null
                    && _httpContext.Request.ClientCertificate.IsPresent)
                {
                    _env.ClientCert = new X509Certificate2(_httpContext.Request.ClientCertificate.Certificate);
                }
            }
            catch (CryptographicException ce)
            {
                Trace.WriteError(Resources.Trace_ClientCertException, ce);
            }
            return Utils.CompletedTask;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is thrown async")]
        private Task SendFileAsync(string name, long offset, long? count, CancellationToken cancel)
        {
            if (cancel.IsCancellationRequested)
            {
                return Utils.CancelledTask;
            }

            try
            {
                OnStart();
                // TransmitFile is not safe to call on a background thread.  It should complete quickly so long as buffering is enabled.
                _httpContext.Response.TransmitFile(name, offset, count ?? -1);

                return Utils.CompletedTask;
            }
            catch (Exception ex)
            {
                return Utils.CreateFaultedTask(ex);
            }
        }

        public void OnStart()
        {
            Exception exception = LazyInitializer.EnsureInitialized(
                ref _startException,
                ref _startCalled,
                ref _startLock,
                StartOnce);

            if (exception != null)
            {
                throw new InvalidOperationException(string.Empty, exception);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Re-thrown outside EnsureInitialized")]
        private Exception StartOnce()
        {
            try
            {
                _sendingHeadersEvent.Fire();

                _headersSent = true;
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Passed to callback")]
        public void OnEnd()
        {
            try
            {
                OnStart();
            }
            catch (Exception ex)
            {
                Complete(ErrorState.Capture(ex));
                return;
            }
            Complete();
        }

        private void Complete()
        {
            AsyncResult.Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, null);
        }

        private void Complete(ErrorState errorState)
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, errorState);
        }

        internal void Complete(bool sync, ErrorState errorState)
        {
            AbortIfHeaderSent();
            AsyncResult.Complete(sync, errorState);
        }

        // Prevent IIS from injecting HTML error details into response bodies that are already in progress.
        internal void AbortIfHeaderSent()
        {
            if (_headersSent)
            {
                _httpRequest.Abort();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disconnectWatcher.Dispose();
            }
        }
    }
}
