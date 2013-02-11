// <copyright file="OwinCallContext.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext : IDisposable
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.OwinCallContext";

        private readonly ITrace _trace;
        private static string _hostAppName;

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

        private Exception _startException;
        private bool _startCalled;
        private object _startLock = new object();

        internal OwinCallContext(
            OwinAppContext appContext,
            RequestContext requestContext,
            string requestPathBase,
            string requestPath,
            AsyncCallback cb,
            object extraData)
        {
            _trace = TraceFactory.Create(TraceName);

            _appContext = appContext;
            _requestContext = requestContext;
            _requestPathBase = requestPathBase;
            _requestPath = requestPath;

            AsyncResult = new CallContextAsyncResult(this, cb, extraData);

            _httpContext = _requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;
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
                            Complete(appTask.Exception);
                        }
                        else if (appTask.IsCanceled)
                        {
                            Complete(new TaskCanceledException(appTask));
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

        internal X509Certificate LoadClientCert()
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
                    _trace.WriteError(Resources.Trace_ClientCertException, ce);
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
                _trace.WriteError(Resources.Trace_ClientCertException, ce);
            }
            return TaskHelpers.Completed();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is thrown async")]
        private Task SendFileAsync(string name, long offset, long? count, CancellationToken cancel)
        {
            if (cancel.IsCancellationRequested)
            {
                return TaskHelpers.Canceled();
            }

            try
            {
                OnStart();
                // TransmitFile is not safe to call on a background thread.  It should complete quickly so long as buffering is enabled.
                _httpContext.Response.TransmitFile(name, offset, count ?? -1);

                return TaskHelpers.Completed();
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }
        }

        private void OnStart()
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

                int statusCode = _env.ResponseStatusCode;
                if (statusCode != default(int))
                {
                    _httpResponse.StatusCode = statusCode;
                }

                string reasonPhrase = _env.ResponseReasonPhrase;
                if (!string.IsNullOrEmpty(reasonPhrase))
                {
                    _httpResponse.StatusDescription = reasonPhrase;
                }

                foreach (var header in _env.ResponseHeaders)
                {
                    int count = header.Value.Length;
                    for (int index = 0; index != count; ++index)
                    {
                        _httpResponse.AddHeader(header.Key, header.Value[index]);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Passed to callback")]
        private void OnEnd()
        {
            try
            {
                OnStart();
            }
            catch (Exception ex)
            {
                Complete(ex);
                return;
            }
            Complete();
        }

        private void Complete()
        {
            AsyncResult.Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, null);
        }

        private void Complete(Exception ex)
        {
            AsyncResult.Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, ex);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnbindDisconnectNotification();
            }
        }
    }
}
