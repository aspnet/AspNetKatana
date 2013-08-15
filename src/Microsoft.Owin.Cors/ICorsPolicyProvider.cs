// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using System.Web.Cors;

namespace Microsoft.Owin.Cors
{
    public interface ICorsPolicyProvider
    {
        Task<CorsPolicy> GetCorsPolicyAsync(IOwinRequest request);
    }
}
