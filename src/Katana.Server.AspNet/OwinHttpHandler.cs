using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Web;
using Owin;

namespace Katana.Server.AspNet
{
    public class OwinHttpHandler : IHttpAsyncHandler
    {
        private readonly Func<AppDelegate> _appAccessor;

        public OwinHttpHandler()
        {
        }

        public OwinHttpHandler(AppDelegate app)
            : this(() => app)
        {
        }

        public OwinHttpHandler(Func<AppDelegate> appAccessor)
        {
            _appAccessor = appAccessor;
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

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

        public IAsyncResult BeginProcessRequest(HttpContextBase context, AsyncCallback cb, object extraData)
        {
            var result = new AsyncResult(cb, extraData);
            try
            {
                var app = _appAccessor.Invoke();
                if (app == null)
                {
                    throw new NullReferenceException("OwinHttpHandler cannot invoke a null app delegate");
                }
                IDictionary<string, object> env = new Dictionary<string, object>();
                app.Invoke(
                    env,
                    (status, headers, body) => result.Complete(false,null),
                    exception => result.Complete(false, exception));
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

            public bool IsCompleted { get; private set; }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public object AsyncState { get; private set; }

            public bool CompletedSynchronously { get; private set; }

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
