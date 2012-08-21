using System;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.Owin.CallEnvironment;
using Microsoft.AspNet.Owin.CallHeaders;
using Owin;

// ReSharper disable AccessToModifiedClosure

namespace Microsoft.AspNet.Owin
{
    public class OwinHttpHandler : IHttpAsyncHandler
    {
        private readonly string _pathBase;
        private readonly Func<AppDelegate> _appAccessor;

        public OwinHttpHandler()
        {
            _pathBase = Utils.NormalizePath(HttpRuntime.AppDomainAppVirtualPath);
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

        // REVIEW: public properties here are extremely bad. overload ctor instead.
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
            // REVIEW: the httpContext.Request.RequestContext may be used here if public property unassigned?

            var callContext = new OwinCallContext(cb, extraData);
            try
            {
                var requestContext = RequestContext ?? new RequestContext(httpContext, new RouteData());
                var requestPathBase = _pathBase;
                var requestPath = RequestPath ?? httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(1) + httpContext.Request.PathInfo;

                var app = _appAccessor.Invoke();
                if (app == null)
                {
                    throw new NullReferenceException("OwinHttpHandler cannot invoke a null app delegate");
                }

                callContext.Execute(requestContext, requestPathBase, requestPath, app);
            }
            catch (Exception ex)
            {
                callContext.Complete(ex);
            }
            return callContext;
        }


        public void EndProcessRequest(IAsyncResult result)
        {
            OwinCallContext.End(result);
        }
    }
}
