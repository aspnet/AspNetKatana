// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    /// <summary>
    /// Determines if the request should take a specific branch of the pipeline by passing the environment
    /// to a user defined callback.
    /// </summary>
    public class MapWhenMiddleware : OwinMiddleware
    {
        private readonly MapWhenOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapWhenMiddleware"/> class
        /// </summary>
        /// <param name="next">The normal application pipeline</param>
        /// <param name="options"></param>
        public MapWhenMiddleware(OwinMiddleware next, MapWhenOptions options)
            : base(next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            _options = options;
        }

#if NET40
        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (_options.Predicate(context))
            {
                return _options.Branch.Invoke(context);
            }
            else
            {
                return Next.Invoke(context);
            }
        }
#else
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
            if (_options.Predicate != null)
            {
                if (_options.Predicate(context))
                {
                    await _options.Branch.Invoke(context);
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
            else
            {
                if (await _options.PredicateAsync(context))
                {
                    await _options.Branch.Invoke(context);
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
        }
#endif
    }
}
