// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public interface IBearerAuthenticationProvider
    {
        /// <summary></summary>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IBearerAuthenticationResult> AuthenticateAsync(string token, CancellationToken cancellationToken);
    }
}
