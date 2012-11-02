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
using Microsoft.Owin.Host.SystemWeb.WebSockets;

namespace Microsoft.Owin.Host.SystemWeb
{
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task /* Complete */>;

    internal partial class OwinCallContext : IDisposable
    {
#if NET40
        private static readonly Action<object> ConnectionTimerCallback = CheckIsClientConnected;
#else
        private static readonly Action<object> SetDisconnectedCallback = SetDisconnected;
#endif

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
        private CancellationTokenSource _callCancelledSource;

#if !NET40
        private WebSocketFunc _webSocketFunc;
        private IDictionary<string, object> _acceptOptions;
#endif

        private CancellationTokenRegistration _connectionCheckRegistration;
        private IDisposable _connectionCheckTimer = null;

        internal void Execute(RequestContext requestContext, string requestPathBase, string requestPath, Func<IDictionary<string, object>, Task> app)
        {
            _requestContext = requestContext;
            _httpContext = requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;
            _callCancelledSource = new CancellationTokenSource();

            PopulateEnvironment(requestPathBase, requestPath);

            RegisterForDisconnectNotification();

            _completedSynchronouslyThreadId = Thread.CurrentThread.ManagedThreadId;
            app.Invoke(_env)
                .Then(() =>
                {
#if !NET40
                    if (_webSocketFunc != null && _env.ResponseStatusCode == 101)
                    {
                        WebSocketHelpers.DoWebSocketUpgrade(_httpContext, _env, _webSocketFunc, _acceptOptions);
                    }
#endif
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
            var requestQueryString = String.Empty;
            if (_httpRequest.Url != null)
            {
                var query = _httpRequest.Url.Query;
                if (query.Length > 1)
                {
                    // pass along the query string without the leading "?" character
                    requestQueryString = query.Substring(1);
                }
            }

            _env = new AspNetDictionary();

            _env.OwinVersion = "1.0";
            _env.CallCancelled = _callCancelledSource.Token;
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
            _env.ResponseBody = new OutputStream(_httpResponse, _httpResponse.OutputStream, OnStart);
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

#if !NET40
            if (WebSocketHelpers.IsAspNetWebSocketRequest(_httpContext))
            {
                _env.WebSocketAccept = new WebSocketAccept(
                    (options, callback) =>
                    {
                        _env.ResponseStatusCode = 101;
                        _acceptOptions = options;
                        _webSocketFunc = callback;
                    });
            }
#endif

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
            catch (CryptographicException)
            {
                // TODO: LOG
            }
            return TaskHelpers.Completed();
        }

        private Task SendFileAsync(string name, long offset, long? count)
        {
            OnStart();
            return Task.Factory.StartNew(() => _httpContext.Response.TransmitFile(name, offset, count ?? -1));
        }

        private void RegisterForDisconnectNotification()
        {
#if NET40
            _connectionCheckTimer = SharedTimer.StaticTimer.Register(ConnectionTimerCallback, this);
#else
            _connectionCheckRegistration = _httpContext.Response.ClientDisconnectedToken.Register(SetDisconnectedCallback, _callCancelledSource);
#endif
        }

#if NET40
        private static void CheckIsClientConnected(object obj)
        {
            OwinCallContext context = (OwinCallContext)obj;
            if (!context._httpResponse.IsClientConnected)
            {
                context._connectionCheckTimer.Dispose();
                SetDisconnected(context._callCancelledSource);
            }
        }
#endif

        private static void SetDisconnected(object obj)
        {
            CancellationTokenSource cts = (CancellationTokenSource)obj;
            try
            {
                cts.Cancel(throwOnFirstException: false);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException)
            {
                // TODO: Log
            }
        }

        private void OnStart()
        {
            var ignored = 0;
            LazyInitializer.EnsureInitialized(
                ref ignored,
                ref _startCalled,
                ref _startLock,
                StartOnce);
        }

        private int StartOnce()
        {
            _sendingHeadersEvent.Fire();

            var statusCode = _env.ResponseStatusCode;
            if (statusCode != default(int))
            {
                _httpResponse.StatusCode = statusCode;
            }

            var reasonPhrase = _env.ResponseReasonPhrase;
            if (!string.IsNullOrEmpty(reasonPhrase))
            {
                _httpResponse.StatusDescription = reasonPhrase;
            }

            foreach (var header in _env.ResponseHeaders)
            {
                var count = header.Value.Length;
                for (var index = 0; index != count; ++index)
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
            if (_callCancelledSource != null)
            {
                try
                {
                    _callCancelledSource.Cancel(throwOnFirstException: false);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (AggregateException)
                {
                    // TODO: LOG
                }
            }
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
                _callCancelledSource.Dispose();
                if (_connectionCheckTimer != null)
                {
                    _connectionCheckTimer.Dispose();
                }
                _connectionCheckRegistration.Dispose();
            }
        }
    }
}
