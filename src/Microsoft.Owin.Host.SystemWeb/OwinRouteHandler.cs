// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Owin;

namespace Microsoft.Owin.Host.SystemWeb
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Processes a route through an OWIN pipeline.
    /// </summary>
    public class OwinRouteHandler : IRouteHandler
    {
        private readonly string _pathBase;
        private readonly string _path;
        private readonly Func<OwinAppContext> _appAccessor;

        /// <summary>
        /// Initialize an OwinRouteHandler
        /// </summary>
        /// <param name="startup">The method to initialize the pipeline that processes requests for the route.</param>
        public OwinRouteHandler(Action<IAppBuilder> startup)
        {
            _pathBase = Utils.NormalizePath(HttpRuntime.AppDomainAppVirtualPath);

            OwinAppContext app = null;
            bool initialized = false;
            var syncLock = new object();
            _appAccessor = () => LazyInitializer.EnsureInitialized(
                ref app,
                ref initialized,
                ref syncLock,
                () => OwinBuilder.Build(startup));
        }

        internal OwinRouteHandler(string pathBase, string path, Func<OwinAppContext> appAccessor)
        {
            _pathBase = pathBase;
            _path = path;
            _appAccessor = appAccessor;
        }

        /// <summary>
        /// Provides the object that processes the request.
        /// </summary>
        /// <returns>
        /// An object that processes the request.
        /// </returns>
        /// <param name="requestContext">An object that encapsulates information about the request.</param>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return ((IRouteHandler)this).GetHttpHandler(requestContext);
        }

        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            return new OwinHttpHandler(_pathBase, _appAccessor, requestContext, _path);
        }
    }
}
