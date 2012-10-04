//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Microsoft.AspNet.Owin
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    public class OwinRouteHandler : IRouteHandler
    {
        private readonly string _pathBase;
        private readonly string _path;
        private readonly Func<AppDelegate> _appAccessor;

        public OwinRouteHandler(string pathBase, string path, Func<AppDelegate> appAccessor)
        {
            _pathBase = pathBase;
            _path = path;
            _appAccessor = appAccessor;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new OwinHttpHandler(_pathBase, _appAccessor) { RequestContext = requestContext, RequestPath = _path };
        }
    }
}
