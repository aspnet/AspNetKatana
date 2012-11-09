// <copyright file="OwinCallContext.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext : IDisposable
    {
        private static string _hostAppName;

        private readonly SendingHeadersEvent _sendingHeadersEvent = new SendingHeadersEvent();

        private readonly OwinAppContext _appContext;
        private readonly RequestContext _requestContext;
        private readonly string _requestPathBase;
        private readonly string _requestPath;
        private HttpContextBase _httpContext;
        private HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private int _completedSynchronouslyThreadId;
        private AspNetDictionary _env;

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
            _appContext = appContext;
            _requestContext = requestContext;
            _requestPathBase = requestPathBase;
            _requestPath = requestPath;
            _cb = cb ?? NoopAsyncCallback;
            AsyncState = extraData;

            _httpContext = _requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;
        }

        internal AspNetDictionary Environment
        {
            get { return _env; }
        }

        internal void Execute()
        {
            CreateEnvironment();

            _completedSynchronouslyThreadId = Thread.CurrentThread.ManagedThreadId;
            _appContext.AppFunc(_env)
                .Then((Action)OnEnd)
                .Catch(errorInfo =>
                {
                    Complete(errorInfo.Exception);
                    return errorInfo.Handled();
                });
            _completedSynchronouslyThreadId = Int32.MinValue;
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
                    Trace.WriteLine(Resources.Exception_ClientCert);
                    Trace.WriteLine(ce.ToString());
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
                Trace.WriteLine(Resources.Exception_ClientCert);
                Trace.WriteLine(ce.ToString());
            }
            return TaskHelpers.Completed();
        }

        private Task SendFileAsync(string name, long offset, long? count, CancellationToken cancel)
        {
            OnStart();
            return Task.Factory.StartNew(() => _httpContext.Response.TransmitFile(name, offset, count ?? -1), cancel);
        }

        private void OnStart()
        {
            int ignored = 0;
            LazyInitializer.EnsureInitialized(
                ref ignored,
                ref _startCalled,
                ref _startLock,
                StartOnce);
        }

        private int StartOnce()
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
            return 0;
        }

        private void OnEnd()
        {
            OnStart();
            Complete();
        }

        internal void Complete()
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, null);
        }

        internal void Complete(Exception ex)
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, ex);
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
