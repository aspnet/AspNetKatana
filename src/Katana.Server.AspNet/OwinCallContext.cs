using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.Owin.CallEnvironment;
using Microsoft.AspNet.Owin.CallHeaders;
using Microsoft.AspNet.Owin.CallStreams;
using Owin;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Owin
{
    #pragma warning disable 811
    using WebSocketFunc =
        Func
        <
        // SendAsync
            Func
            <
                ArraySegment<byte> /* data */,
                int /* messageType */,
                bool /* endOfMessage */,
                CancellationToken /* cancel */,
                Task
            >,
        // ReceiveAsync
            Func
            <
                ArraySegment<byte> /* data */,
                CancellationToken /* cancel */,
                Task
                <
                    Tuple
                    <
                        int /* messageType */,
                        bool /* endOfMessage */,
                        int? /* count */,
                        int? /* closeStatus */,
                        string /* closeStatusDescription */
                    >
                >
            >,
        // CloseAsync
            Func
            <
                int /* closeStatus */,
                string /* closeDescription */,
                CancellationToken /* cancel */,
                Task
            >,
        // Complete
            Task
        >;
    #pragma warning restore 811

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

            CallParameters call = new CallParameters();
            call.Body = _httpRequest.InputStream;
            call.Headers = AspNetRequestHeaders.Create(_httpRequest);
            AspNetDictionary env = new AspNetDictionary()
            {
                OwinVersion = "1.0",
                HttpVersion = _httpRequest.ServerVariables["SERVER_PROTOCOL"],
                RequestScheme = _httpRequest.IsSecureConnection ? "https" : "http",
                RequestMethod = _httpRequest.HttpMethod,
                RequestPathBase = requestPathBase,
                RequestPath = requestPath,
                RequestQueryString = requestQueryString,
                CallCompleted = CallCompleted,

                HostTraceOutput = TraceTextWriter.Instance,
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

            call.Environment = env;

            _completedSynchronouslyThreadId = Int32.MinValue;
            app.Invoke(call)
                .Then(result => OnResult(result))
                .Catch(errorInfo =>
                {
                    Complete(errorInfo.Exception);
                    return errorInfo.Handled();
                });
            _completedSynchronouslyThreadId = Int32.MinValue;
        }

        private void OnResult(ResultParameters result)
        {
            _httpResponse.StatusCode = result.Status;
            
            object reasonPhrase;
            if (result.Properties != null && result.Properties.TryGetValue(Constants.ReasonPhraseKey, out reasonPhrase))
            {
                _httpResponse.StatusDescription = Convert.ToString(reasonPhrase);
            }

            foreach (var header in result.Headers)
            {
                var count = header.Value.Length;
                for(var index = 0; index !=count; ++index)
                {
                    _httpResponse.AddHeader(header.Key, header.Value[index]);
                }
            }

            if (result.Body != null)
            {
                try
                {
                    var output = new OutputStream(
                        _httpResponse, 
                        _httpResponse.OutputStream);

                    result.Body(output)
                        .Then(() => Complete())
                        .Catch(errorInfo =>
                        {
                            Complete(errorInfo.Exception);
                            return errorInfo.Handled();
                        });
                }
                catch (Exception ex)
                {
                    Complete(ex);
                }
            }
            else
            {
                Complete();
            }
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
