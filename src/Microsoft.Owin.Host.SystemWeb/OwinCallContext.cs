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
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;
using Microsoft.Owin.Host.SystemWeb.CallStreams;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinCallContext : IDisposable
    {
        private static string _hostAppName;

        private readonly SendingHeadersEvent _sendingHeadersEvent = new SendingHeadersEvent();

        private RequestContext _requestContext;
        private HttpContextBase _httpContext;
        private HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private int _completedSynchronouslyThreadId;
        private AspNetDictionary _env;

        private bool _startCalled;
        private object _startLock = new object();

        internal void Execute(RequestContext requestContext, string requestPathBase, string requestPath, Func<IDictionary<string, object>, Task> app)
        {
            _requestContext = requestContext;
            _httpContext = requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;

            PopulateEnvironment(requestPathBase, requestPath);

            _completedSynchronouslyThreadId = Thread.CurrentThread.ManagedThreadId;
            app.Invoke(_env)
                .Then(() =>
                {
                    DoWebSocketUpgrade();
                    OnEnd();
                })
                .Catch(errorInfo =>
                {
                    Complete(errorInfo.Exception);
                    return errorInfo.Handled();
                });
            _completedSynchronouslyThreadId = Int32.MinValue;
        }

        private void PopulateEnvironment(string requestPathBase, string requestPath)
        {
            string requestQueryString = String.Empty;
            if (_httpRequest.Url != null)
            {
                string query = _httpRequest.Url.Query;
                if (query.Length > 1)
                {
                    // pass along the query string without the leading "?" character
                    requestQueryString = query.Substring(1);
                }
            }

            _env = new AspNetDictionary();

            _env.OwinVersion = "1.0";
            _env.CallCancelled = BindDisconnectNotification();
            _env.OnSendingHeaders = _sendingHeadersEvent.Register;
            _env.RequestScheme = _httpRequest.IsSecureConnection ? "https" : "http";
            _env.RequestMethod = _httpRequest.HttpMethod;
            _env.RequestPathBase = requestPathBase;
            _env.RequestPath = requestPath;
            _env.RequestQueryString = requestQueryString;
            _env.RequestProtocol = _httpRequest.ServerVariables["SERVER_PROTOCOL"];
            _env.RequestHeaders = AspNetRequestHeaders.Create(_httpRequest);
            _env.RequestBody = _httpRequest.InputStream;
            _env.ResponseHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            _env.ResponseBody = new OutputStream(_httpResponse, _httpResponse.OutputStream, OnStart, OnFaulted);
            _env.SendFileAsync = SendFileAsync;
            _env.HostTraceOutput = TraceTextWriter.Instance;
            _env.HostAppName = LazyInitializer.EnsureInitialized(ref _hostAppName,
                () => HostingEnvironment.SiteName ?? new Guid().ToString());
            _env.ServerDisableResponseBuffering = DisableResponseBuffering;
            _env.ServerUser = _httpContext.User;
            _env.ServerIsLocal = _httpRequest.IsLocal;
            _env.ServerLocalIpAddress = _httpRequest.ServerVariables["LOCAL_ADDR"];
            _env.ServerLocalPort = _httpRequest.ServerVariables["SERVER_PORT"];
            _env.ServerRemoteIpAddress = _httpRequest.ServerVariables["REMOTE_ADDR"];
            _env.ServerRemotePort = _httpRequest.ServerVariables["REMOTE_PORT"];
            _env.RequestContext = _requestContext;
            _env.HttpContextBase = _httpContext;
            _env.WebSocketAccept = BindWebSocketAccept();

            if (_httpContext.Request.IsSecureConnection)
            {
                _env.LoadClientCert = LoadClientCertAsync;
            }

            if (GetIsDebugEnabled(_httpContext))
            {
                _env.HostAppMode = Constants.AppModeDevelopment;
            }
        }

        private static bool GetIsDebugEnabled(HttpContextBase context)
        {
            try
            {
                // Not implemented by custom classes or unit tests fakes.
                return context.IsDebuggingEnabled;
            }
            catch (NotImplementedException)
            {
            }
            return false;
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

        private Task SendFileAsync(string name, long offset, long? count)
        {
            OnStart();
            return Task.Factory.StartNew(() => _httpContext.Response.TransmitFile(name, offset, count ?? -1));
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
