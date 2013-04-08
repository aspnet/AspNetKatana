// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Basic
{
    internal interface IBasicAuthenticationProtocol
    {
        Task<IBasicAuthenticationResult> AuthenticateAsync(AuthenticationHeaderValue authorization,
            CancellationToken cancellationToken);

        Task<AuthenticationHeaderValue> CreateChallengeAsync(CancellationToken cancellationToken);
    }
}
