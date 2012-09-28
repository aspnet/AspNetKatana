using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using Microsoft.AspNet.Owin.CallEnvironment;
using Microsoft.AspNet.Owin.CallHeaders;
using Microsoft.AspNet.Owin.CallStreams;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Owin
{
    public partial class OwinCallContext
    {
        private HttpContextBase _httpContext;
        private HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private int _completedSynchronouslyThreadId;
        AspNetDictionary _env;
        readonly SendingHeadersEvent _sendingHeadersEvent = new SendingHeadersEvent();

        bool _startCalled;
        object _startLock = new object();
        CancellationTokenSource _callCancelledSource;
        static string _hostAppName;

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
                ServerName = Constants.ServerName,
                ServerVersion = Constants.ServerVersion,
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

                ResponseHeaders = new Dictionary<string, string[]>(StringComparer.InvariantCultureIgnoreCase),
                ResponseBody = new OutputStream(_httpResponse, _httpResponse.OutputStream, OnStart),

                SendFileVersion = Constants.SendFileVersion,
                SendFileSupport = Constants.SendFileSupport,
                SendFileFunc = SendFile,
                // No overlapped WriteFile support

                HostTraceOutput = TraceTextWriter.Instance,
                HostAppName = LazyInitializer.EnsureInitialized(ref _hostAppName, () => HostingEnvironment.SiteName),
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

            _completedSynchronouslyThreadId = Int32.MinValue;
            app.Invoke(_env)
                .Then(() => OnEnd())
                .Catch(errorInfo =>
                {
                    _callCancelledSource.Cancel(false);
                    Complete(errorInfo.Exception);
                    return errorInfo.Handled();
                });
            _completedSynchronouslyThreadId = Int32.MinValue;
        }

        private Task SendFile(string name, long offset, long? count)
        {
            OnStart();
            return Task.Factory.StartNew(() => this._httpContext.Response.TransmitFile(name, offset, count ?? -1));
        }

        void CheckIsClientConnected()
        {
            if (!_httpResponse.IsClientConnected && !_callCancelledSource.IsCancellationRequested)
            {
                // notify interested 
                _callCancelledSource.Cancel(false);
            }
        }

        void OnStart()
        {
            var ignored = 0;
            LazyInitializer.EnsureInitialized(
                ref ignored,
                ref _startCalled,
                ref _startLock,
                StartOnce);
        }

        int StartOnce()
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

        void OnEnd()
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
    }
}
