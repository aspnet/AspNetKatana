using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Katana.Server.AspNet.CallEnvironment;
using Katana.Server.AspNet.CallHeaders;
using Owin;

namespace Katana.Server.AspNet
{
    public partial class OwinCallContext
    {
        private HttpContextBase _httpContext;
        private HttpRequestBase _httpRequest;
        private HttpResponseBase _httpResponse;
        private int _completedSynchronouslyThreadId;

        public void Execute(RequestContext requestContext, string requestPathBase, string requestPath, AppDelegate app)
        {
            _httpContext = requestContext.HttpContext;
            _httpRequest = _httpContext.Request;
            _httpResponse = _httpContext.Response;

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

            var env = new AspNetDictionary
            {
                OwinVersion = "1.0",
                HttpVersion = _httpRequest.ServerVariables["SERVER_PROTOCOL"],
                RequestScheme = _httpRequest.IsSecureConnection ? "https" : "http",
                RequestMethod = _httpRequest.HttpMethod,
                RequestPathBase = requestPathBase,
                RequestPath = requestPath,
                RequestQueryString = requestQueryString,
                RequestHeaders = AspNetRequestHeaders.Create(_httpRequest),
                RequestBody = null,

                ServerVariableLocalAddr = _httpRequest.ServerVariables["LOCAL_ADDR"],
                ServerVariableRemoteAddr = _httpRequest.ServerVariables["REMOTE_ADDR"],
                ServerVariableRemoteHost = _httpRequest.ServerVariables["REMOTE_HOST"],
                ServerVariableRemotePort = _httpRequest.ServerVariables["REMOTE_PORT"],
                ServerVariableServerPort = _httpRequest.ServerVariables["SERVER_PORT"],

                HostCallDisposed = CallDisposed,
                HostDisableResponseBuffering = DisableResponseBuffering,
                HostUser = _httpContext.User,

                RequestContext = requestContext,
                HttpContextBase = _httpContext,
            };
            
            _completedSynchronouslyThreadId = Int32.MinValue;
            app.Invoke(env, OnResult, OnFault);
            _completedSynchronouslyThreadId = Int32.MinValue;
        }

        private void OnResult(string status, IDictionary<string, string[]> headers, BodyDelegate body)
        {
            _httpResponse.Status = status;
            foreach (var header in headers)
            {
                foreach (var value in header.Value)
                {
                    _httpResponse.AddHeader(header.Key, value);
                }
            }

            if (body != null)
            {
                body(OnWrite, OnEnd, CallDisposed);
            }
            else
            {
                Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, null);
            }
        }

        private void OnFault(Exception ex)
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, ex);
        }


        private bool OnWrite(ArraySegment<byte> data, Action callback)
        {
            return data.Array == null
                       ? (callback == null ? SyncFlush() : AsyncFlush(callback))
                       : (callback == null ? SyncWrite(data) : AsyncWrite(data, callback));
        }

        private bool SyncWrite(ArraySegment<byte> data)
        {
            _httpResponse.OutputStream.Write(data.Array, data.Offset, data.Count);
            return false;
        }

        private bool AsyncWrite(ArraySegment<byte> data, Action callback)
        {
            SyncWrite(data);
            return AsyncFlush(callback);
        }

        private bool SyncFlush()
        {
            _httpResponse.Flush();
            return false;
        }

        private bool AsyncFlush(Action callback)
        {
#if NET45
            if (response.SupportsAsyncFlush)
            {
                return Task.Factory.FromAsync((cb, state) => response.BeginFlush(cb, state), ar => response.EndFlush(ar), null);
            }

            response.Flush();
            return TaskAsyncHelper.Empty;
#else
            // TODO: if NET40-on-NET45, also try to invoke above logic dynamically, otherwise

            // NET40-on-NET40
            _httpResponse.Flush();
            return false;
#endif
        }

        private void OnEnd(Exception ex)
        {
            Complete(_completedSynchronouslyThreadId == Thread.CurrentThread.ManagedThreadId, ex);
        }
    }
}
