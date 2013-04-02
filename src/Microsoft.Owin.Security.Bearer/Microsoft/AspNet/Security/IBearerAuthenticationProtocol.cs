// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public interface IBearerAuthenticationProtocol
    {
        /// <summary></summary>
        /// <param name="authorization"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IBearerAuthenticationResult> AuthenticateAsync(AuthenticationHeaderValue authorization,
            CancellationToken cancellationToken);

        /// <summary></summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<AuthenticationHeaderValue> CreateChallengeAsync(CancellationToken cancellationToken);
    }
}
