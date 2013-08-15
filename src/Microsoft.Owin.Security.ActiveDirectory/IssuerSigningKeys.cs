// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.ActiveDirectory
{
    /// <summary>
    /// Signing metadata parsed from a WSFed endpoint.
    /// </summary>
    internal class IssuerSigningKeys
    {
        /// <summary>
        /// The token issuer.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Signing tokens.
        /// </summary>
        public IEnumerable<X509SecurityToken> Tokens { get; set; }
    }
}
