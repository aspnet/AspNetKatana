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
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
#if NET45
using System.Web.WebSockets;
#endif
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.CallHeaders;
using Microsoft.Owin.Host.SystemWeb.CallStreams;
#if NET45
using Microsoft.Owin.Host.SystemWeb.WebSockets;
#endif

namespace Microsoft.Owin.Host.SystemWeb
{
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task /* Complete */>;

    public partial class OwinCallContext : IDisposable
    {
        private static string _hostAppName;

        private readonly SendingHeadersEvent _sendingHeadersEvent = new SendingHeadersEvent();

        private HttpContextBase _httpContext;
        private HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private int _completedSynchronouslyThreadId;
        private AspNetDictionary _env;

        private bool _startCalled;
        private object _startLock = new object();
        private CancellationTokenSource _callCancelledSource;
#if NET45
        private WebSocketFunc _webSocketFunc;
        private IDictionary<string, object> _acceptOptions;
#endif

        public void Execute(RequestContext requestContext, string requestPathBase, string requestPath, Func<IDictionary<string, object>, Task> app)
        {
            _httpContext = requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;
            _callCancelledSource = new CancellationTokenSource();

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

            _env = new AspNetDictionary
            {
                OwinVersion = "1.0",
                CallCancelled = _callCancelledSource.Token,
                OnSendingHeaders = _sendingHeadersEvent.Register,
                RequestScheme = _httpRequest.IsSecureConnection ? "https" : "http",
                RequestMethod = _httpRequest.HttpMethod,
                RequestPathBase = requestPathBase,
                RequestPath = requestPath,
                RequestQueryString = requestQueryString,
                RequestProtocol = _httpRequest.ServerVariables["SERVER_PROTOCOL"],
                RequestHeaders = AspNetRequestHeaders.Create(_httpRequest),
                RequestBody = _httpRequest.InputStream,
                ResponseHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
                ResponseBody = new OutputStream(_httpResponse, _httpResponse.OutputStream, OnStart),
                SendFileAsync = SendFileAsync,
                HostTraceOutput = TraceTextWriter.Instance,
                HostAppName = LazyInitializer.EnsureInitialized(ref _hostAppName,
                    () => HostingEnvironment.SiteName ?? new Guid().ToString()),
                ServerDisableResponseBuffering = DisableResponseBuffering,
                ServerUser = _httpContext.User,
                ServerIsLocal = _httpRequest.IsLocal,
                ServerLocalIpAddress = _httpRequest.ServerVariables["LOCAL_ADDR"],
                ServerLocalPort = _httpRequest.ServerVariables["SERVER_PORT"],
                ServerRemoteIpAddress = _httpRequest.ServerVariables["REMOTE_ADDR"],
                ServerRemotePort = _httpRequest.ServerVariables["REMOTE_PORT"],
                RequestContext = requestContext,
                HttpContextBase = _httpContext,
            };

#if NET45
            if (!String.IsNullOrEmpty(_httpContext.Request.ServerVariables[Constants.AspNetServerVariableWebSocketVersion])
                && IsAspNetWebSocketRequest(_httpContext))
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

            _completedSynchronouslyThreadId = Int32.MinValue;
            app.Invoke(_env)
                .Then(() =>
                {
#if NET45
                    if (_webSocketFunc != null && _env.ResponseStatusCode == 101)
                    {
                        var options = new AspNetWebSocketOptions();
                        options.SubProtocol = GetWebSocketSubProtocol();

                        _httpContext.AcceptWebSocketRequest(async webSocketContext =>
                        {
                            try
                            {
                                var wrapper = new OwinWebSocketWrapper(webSocketContext, _callCancelledSource.Token);
                                await _webSocketFunc(wrapper.Environment);
                                await wrapper.CleanupAsync();
                            }
                            catch (Exception)
                            {
                                // TODO: Log
                                throw;
                            }
                        }, options);
                    }
#endif
                    OnEnd();
                })
                .Catch(errorInfo =>
                {
                    _callCancelledSource.Cancel(false);
                    Complete(errorInfo.Exception);
                    return errorInfo.Handled();
                });
            _completedSynchronouslyThreadId = Int32.MinValue;
        }
        
#if NET45
        private static bool IsAspNetWebSocketRequest(HttpContextBase context)
        {
            bool isWebSocketRequest = false;
            if (context != null)
            {
                // Not implemented by custom contexts or FakeN.Web.
                try
                {
                    if (context.IsWebSocketRequest)
                    {
                        isWebSocketRequest = true;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            return isWebSocketRequest;
        }
#endif

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
            catch (Exception)
            {
            }
            return TaskHelpers.Completed();
        }

        private Task SendFileAsync(string name, long offset, long? count)
        {
            OnStart();
            return Task.Factory.StartNew(() => _httpContext.Response.TransmitFile(name, offset, count ?? -1));
        }

        private void CheckIsClientConnected()
        {
            if (!_httpResponse.IsClientConnected && !_callCancelledSource.IsCancellationRequested)
            {
                // notify interested 
                _callCancelledSource.Cancel(false);
            }
        }

#if NET45
        private string GetWebSocketSubProtocol()
        {
            IDictionary<string, string[]> reponseHeaders = _env.ResponseHeaders;

            // Remove the subprotocol header, Accept will re-add it.
            string subProtocol = null;
            string[] subProtocols;
            if (reponseHeaders.TryGetValue(Constants.SecWebSocketProtocol, out subProtocols) && subProtocols.Length > 0)
            {
                subProtocol = subProtocols[0];
                reponseHeaders.Remove(Constants.SecWebSocketProtocol);
            }

            if (_acceptOptions != null && _acceptOptions.ContainsKey(Constants.WebSocketSubProtocolKey))
            {
                subProtocol = _acceptOptions.Get<string>(Constants.WebSocketSubProtocolKey);
            }

            return subProtocol;
        }
#endif

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

        public void Complete()
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, null);
        }

        public void Complete(Exception ex)
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
                _callCancelledSource.Dispose();
            }
        }
    }
}
