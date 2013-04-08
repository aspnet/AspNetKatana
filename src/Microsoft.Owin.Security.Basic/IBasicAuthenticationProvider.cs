// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Basic
{
    /// <summary></summary>
    public interface IBasicAuthenticationProvider
    {
        /// <summary></summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken);
    }
}
