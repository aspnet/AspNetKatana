// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Bearer
{
    internal interface IBearerAuthenticationProtocol
    {
        Task<IBearerAuthenticationResult> AuthenticateAsync(AuthenticationHeaderValue authorization,
            CancellationToken cancellationToken);

        Task<AuthenticationHeaderValue> CreateChallengeAsync(CancellationToken cancellationToken);
    }
}
