// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Provides security key information to the implementing class.
    /// </summary>
    public interface IIssuerSecurityKeyProvider
    {
        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        string Issuer { get; }

        /// <summary>
        /// Gets all known security keys.
        /// </summary>
        /// <value>
        /// All known security keys.
        /// </value>
        IEnumerable<SecurityKey> SecurityKeys { get; }
    }
}
