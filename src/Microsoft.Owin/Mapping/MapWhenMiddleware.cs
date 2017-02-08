// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Mapping
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Determines if the request should take a specific branch of the pipeline by passing the environment
    /// to a user defined callback.
    /// </summary>
    public class MapWhenMiddleware
    {
        private readonly AppFunc _next;
        private readonly MapWhenOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapWhenMiddleware"/> class
        /// </summary>
        /// <param name="next">The normal application pipeline</param>
        /// <param name="options"></param>
        public MapWhenMiddleware(AppFunc next, MapWhenOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            _next = next;
            _options = options;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public async Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);

            if (_options.Predicate != null)
            {
                if (_options.Predicate(context))
                {
                    await _options.Branch(environment);
                }
                else
                {
                    await _next(environment);
                }
            }
            else
            {
                if (await _options.PredicateAsync(context))
                {
                    await _options.Branch(environment);
                }
                else
                {
                    await _next(environment);
                }
            }
        }
    }
}
