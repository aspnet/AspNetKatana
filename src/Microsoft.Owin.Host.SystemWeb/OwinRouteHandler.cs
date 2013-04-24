// <copyright file="OwinRouteHandler.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

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
            _appAccessor = () => OwinBuilder.Build(startup);
        }

        /// <summary>
        /// Initialize an OwinRouteHandler
        /// </summary>
        /// <param name="pathBase">The value to provide as the request path base instead of the current HttpRuntime.AppDomainAppVirtualPath.</param>
        /// <param name="startup">The method to initialize the pipeline that processes requests for the route.</param>
        [Obsolete("Use MapOwinPath instead.")]
        public OwinRouteHandler(string pathBase, Action<IAppBuilder> startup)
        {
            _pathBase = Utils.NormalizePath(pathBase);

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
