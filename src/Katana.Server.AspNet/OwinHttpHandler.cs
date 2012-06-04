using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Katana.Server.AspNet.CallEnvironment;
using Katana.Server.AspNet.CallHeaders;
using Owin;

// ReSharper disable AccessToModifiedClosure

namespace Katana.Server.AspNet
{
    public class OwinHttpHandler : IHttpAsyncHandler
    {
        private readonly string _pathBase;
        private readonly Func<AppDelegate> _appAccessor;

        public OwinHttpHandler()
        {
        }

        public OwinHttpHandler(string pathBase, AppDelegate app)
            : this(pathBase, () => app)
        {
        }

        public OwinHttpHandler(string pathBase, Func<AppDelegate> appAccessor)
        {
            _pathBase = pathBase;
            _appAccessor = appAccessor;
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public RequestContext RequestContext { get; set; }
        public string RequestPath { get; set; }

        public void ProcessRequest(HttpContextBase context)
        {
            // the synchronous version of this handler must never be called
            throw new NotImplementedException();
        }

        public bool IsReusable
        {
            get { return true; }
        }


        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return BeginProcessRequest(new HttpContextWrapper(context), cb, extraData);
        }

        public IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback cb, object extraData)
        {
            var result = new AsyncResult(cb, extraData);
            try
            {
                var app = _appAccessor.Invoke();
                if (app == null)
                {
                    throw new NullReferenceException("OwinHttpHandler cannot invoke a null app delegate");
                }

                var httpRequest = httpContext.Request;
                var httpResponse = httpContext.Response;


                // environment requestPathBase consist of the vdir combined with the owin route's pathBase 
                var requestPathBase = Utils.NormalizePath(httpRequest.ApplicationPath) + _pathBase;

                // if the owin route matched the request, then the remaining request path is already known
                var requestPath = RequestPath;

                if (requestPath == null)
                {
                    // otherwise the request path will be all of the segments "/xxx/yyy/zzz" minus the leading "~" app relative character
                    requestPath = httpRequest.AppRelativeCurrentExecutionFilePath.Substring(1) + httpRequest.PathInfo;
                }

                var requestQueryString = string.Empty;
                if (httpRequest.Url != null)
                {
                    var query = httpRequest.Url.Query;
                    if (query.Length > 1)
                    {
                        // pass along the query string without the leading "?" character
                        requestQueryString = query.Substring(1);
                    }
                }
                
                var env = new AspNetDictionary
                              {
                                  OwinVersion = "1.1",
                                  RequestScheme = httpRequest.IsSecureConnection ? "https" : "http",
                                  RequestMethod = httpRequest.HttpMethod,
                                  RequestPathBase = requestPathBase,
                                  RequestPath = requestPath,
                                  RequestQueryString = requestQueryString,
                                  RequestHeaders = AspNetRequestHeaders.Create(httpRequest),
                                  RequestBody = null,
                                  
                                  CallDisposed = result.CallDisposed,

                                  RequestContext = RequestContext,
                                  HttpContextBase = httpContext,
                              };

                var managedThreadId = int.MinValue;// Thread.CurrentThread.ManagedThreadId;
                app.Invoke(
                    env,
                    (status, headers, body) =>
                    {
                        if (body != null)
                        {
                            body.Invoke(
                                data =>
                                {
                                    httpResponse.OutputStream.Write(data.Array, data.Offset, data.Count);
                                    return false;
                                },
                                _ => false,
                                ex => result.Complete(managedThreadId == Thread.CurrentThread.ManagedThreadId, ex),
                                result.CallDisposed);
                        }
                        else
                        {
                            result.Complete(managedThreadId == Thread.CurrentThread.ManagedThreadId, null);
                        }
                    },
                    ex => result.Complete(managedThreadId == Thread.CurrentThread.ManagedThreadId, ex));

                managedThreadId = int.MinValue;
            }
            catch (Exception ex)
            {
                result.Complete(true, ex);
            }
            return result;
        }


        public void EndProcessRequest(IAsyncResult result)
        {
            AsyncResult.End(result);
        }

        class AsyncResult : IAsyncResult
        {
            private AsyncCallback _cb;
            private Exception _exception;

            public AsyncResult(AsyncCallback cb, object extraData)
            {
                _cb = cb ?? NoopAsyncCallback;
                AsyncState = extraData;
            }

            private static readonly AsyncCallback NoopAsyncCallback =
                ar => { };

            private static readonly AsyncCallback ExtraAsyncCallback =
                ar => Trace.WriteLine("OwinHttpHandler: more than one call to complete the same AsyncResult");

            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            public bool IsCompleted { get; private set; }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public object AsyncState { get; private set; }

            public bool CompletedSynchronously { get; private set; }

            public CancellationToken CallDisposed { get { return _cancellationTokenSource.Token; } }

            public void Complete(bool completedSynchronously, Exception exception)
            {
                _exception = exception;
                CompletedSynchronously = completedSynchronously;
                IsCompleted = true;
                try
                {
                    Interlocked.Exchange(ref _cb, ExtraAsyncCallback).Invoke(this);
                }
                catch (Exception ex)
                {
                    // TODO: certain exception must never be caught - find out what those are and
                    // rethrow if ex is one of them
                    Trace.WriteLine("OwinHttpHandler: AsyncResult callback threw an exception. " + ex.Message);
                }
                _cancellationTokenSource.Cancel(false);
            }

            public static void End(IAsyncResult result)
            {
                if (!(result is AsyncResult))
                {
                    throw new InvalidOperationException("EndProcessRequest must be called with return value of BeginProcessRequest");
                }
                var self = ((AsyncResult)result);
                if (self._exception != null)
                {
                    throw new TargetInvocationException(self._exception);
                }
            }
        }
    }
}
