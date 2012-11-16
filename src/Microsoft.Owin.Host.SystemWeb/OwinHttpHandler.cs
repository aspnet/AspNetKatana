// <copyright file="OwinHttpHandler.cs" company="Katana contributors">
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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

// ReSharper disable AccessToModifiedClosure

namespace Microsoft.Owin.Host.SystemWeb
{
    public sealed class OwinHttpHandler : IHttpAsyncHandler
    {
        private readonly string _pathBase;
        private readonly Func<OwinAppContext> _appAccessor;
        private readonly RequestContext _requestContext;
        private readonly string _requestPath;

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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Dispose is handled in the callback")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled via callback")]
        public IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, object extraData)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            OwinCallContext callContext = null;
            try
            {
                OwinAppContext appContext = _appAccessor.Invoke();

                // REVIEW: the httpContext.Request.RequestContext may be used here if public property unassigned?
                RequestContext requestContext = _requestContext ?? new RequestContext(httpContext, new RouteData());
                string requestPathBase = _pathBase;
                string requestPath = _requestPath ?? httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(1) + httpContext.Request.PathInfo;

                callContext = appContext.CreateCallContext(
                    requestContext,
                    requestPathBase,
                    requestPath,
                    callback,
                    extraData);

                callContext.Execute();
                return callContext.AsyncResult;
            }
            catch (Exception ex)
            {
                if (callContext != null)
                {
                    callContext.Dispose();
                }
                return TaskHelpers.FromError(ex);
            }
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            var task = result as Task;
            if (task != null)
            {
                task.Wait();
            }
            else
            {
                CallContextAsyncResult.End(result);
            }
        }
    }
}
