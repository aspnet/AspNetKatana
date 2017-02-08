// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    /// <summary>
    /// A pluggable CORS policy provider that always returns null by default.
    /// </summary>
    public class CorsPolicyProvider : ICorsPolicyProvider
    {
        /// <summary>
        /// Creates a new CorsPolicyProvider instance.
        /// </summary>
        public CorsPolicyProvider()
        {
            PolicyResolver = request => Task.FromResult<CorsPolicy>(null);
        }

        /// <summary>
        /// A pluggable callback that will be used to select the CORS policy for the given requests.
        /// </summary>
        public Func<IOwinRequest, Task<CorsPolicy>> PolicyResolver { get; set; }

        /// <summary>
        /// Executes the PolicyResolver unless overridden by a subclass.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual Task<CorsPolicy> GetCorsPolicyAsync(IOwinRequest request)
        {
            return PolicyResolver(request);
        }
    }
}
