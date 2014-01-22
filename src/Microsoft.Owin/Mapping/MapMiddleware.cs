// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    /// <summary>
    /// Used to create path based branches in your application pipeline.
    /// The owin.RequestPathBase is not included in the evaluation, only owin.RequestPath.
    /// Matching paths have the matching piece removed from owin.RequestPath and added to the owin.RequestPathBase.
    /// </summary>
    public class MapMiddleware : OwinMiddleware
    {
        private readonly MapOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapMiddleware"/> class
        /// </summary>
        /// <param name="next">The normal pipeline taken for a negative match</param>
        /// <param name="options"></param>
        public MapMiddleware(OwinMiddleware next, MapOptions options) : base(next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (options.PathMatch == null)
            {
                throw new ArgumentException(Resources.Exception_PathRequired);
            }

            _options = options;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            PathString path = context.Request.Path;

            PathString remainingPath;
            if (path.StartsWithSegments(_options.PathMatch, out remainingPath))
            {
                // Update the path
                PathString pathBase = context.Request.PathBase;
                context.Request.PathBase = pathBase + _options.PathMatch;
                context.Request.Path = remainingPath;

                await _options.Branch.Invoke(context);

                context.Request.PathBase = pathBase;
                context.Request.Path = path;
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }
}
