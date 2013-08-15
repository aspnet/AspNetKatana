// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    public class CorsPolicyProvider : ICorsPolicyProvider
    {
        public CorsPolicyProvider()
        {
            PolicyResolver = request => Task.FromResult<CorsPolicy>(null);
        }

        public Func<IOwinRequest, Task<CorsPolicy>> PolicyResolver { get; set; }

        public virtual Task<CorsPolicy> GetCorsPolicyAsync(IOwinRequest request)
        {
            return PolicyResolver(request);
        }
    }
}
