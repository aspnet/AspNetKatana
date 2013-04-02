// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Principal;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public interface IBearerAuthenticationResult
    {
        /// <summary></summary>
        IPrincipal Principal { get; }

        /// <summary></summary>
        IBearerAuthenticationError ErrorResult { get; }
    }
}
