// <copyright file="OwinHttpHandler.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Routing;

// ReSharper disable AccessToModifiedClosure

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary>
    /// Processes requests from System.Web as OWIN requests.
    /// </summary>
    public sealed class OwinHttpHandler : IHttpAsyncHandler
    {
        private readonly string _pathBase;
        private readonly Func<OwinAppContext> _appAccessor;
        private readonly RequestContext _requestContext;
        private readonly string _requestPath;

        /// <summary>
        /// Processes requests using the default OWIN application.
        /// </summary>
        public OwinHttpHandler()
            : this(Utils.NormalizePath(HttpRuntime.AppDomainAppVirtualPath), OwinApplication.Accessor)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        internal OwinHttpHandler(string pathBase, OwinAppContext app)
            : this(pathBase, () => app)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        internal OwinHttpHandler(string pathBase, Func<OwinAppContext> appAccessor)
        {
            _pathBase = pathBase;
            _appAccessor = appAccessor;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        internal OwinHttpHandler(string pathBase, Func<OwinAppContext> appAccessor, RequestContext context, string path)
            : this(pathBase, appAccessor)
        {
            _requestContext = context;
            _requestPath = path;
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the System.Web.IHttpHandler instance.
        /// </summary>
        /// <returns>
        /// true.
        /// </returns>
        public bool IsReusable
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context", Justification = "Interface method is not implemented")]
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            // the synchronous version of this handler must never be called
            throw new NotImplementedException();
        }

        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return BeginProcessRequest(new HttpContextWrapper(context), cb, extraData);
        }

        /// <summary>
        /// Initiates an asynchronous call to the HTTP handler.
        /// </summary>
        /// <param name="httpContext">
        /// An System.Web.HttpContextBase object that provides references to intrinsic server
        /// objects (for example, Request, Response, Session, and Server) used to service
        /// HTTP requests.
        /// </param>
        /// <param name="callback">
        /// The System.AsyncCallback to call when the asynchronous method call is complete.
        /// If callback is null, the delegate is not called.
        /// </param>
        /// <param name="extraData">
        /// Any extra data needed to process the request.
        /// </param>
        /// <returns>
        /// An System.IAsyncResult that contains information about the status of the process.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are rethrown in the callback")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose is handled in the callback")]
        public IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, object extraData)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            try
            {
                OwinAppContext appContext = _appAccessor.Invoke();

                // REVIEW: the httpContext.Request.RequestContext may be used here if public property unassigned?
                RequestContext requestContext = _requestContext ?? new RequestContext(httpContext, new RouteData());
                string requestPathBase = _pathBase;
                string requestPath = _requestPath ?? httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(1) + httpContext.Request.PathInfo;

                OwinCallContext callContext = appContext.CreateCallContext(
                    requestContext,
                    requestPathBase,
                    requestPath,
                    callback,
                    extraData);

                try
                {
                    callContext.Execute();
                }
                catch (Exception ex)
                {
                    callContext.AsyncResult.Complete(true, ex);
                }
                return callContext.AsyncResult;
            }
            catch (Exception ex)
            {
                var failedAsyncResult = new CallContextAsyncResult(null, callback, extraData);
                failedAsyncResult.Complete(true, ex);
                return failedAsyncResult;
            }
        }

        /// <summary>
        /// Provides an asynchronous process End method when the process ends.
        /// </summary>
        /// <param name="result">
        /// An System.IAsyncResult that contains information about the status of the process.
        /// </param>
        public void EndProcessRequest(IAsyncResult result)
        {
            CallContextAsyncResult.End(result);
        }
    }
}
