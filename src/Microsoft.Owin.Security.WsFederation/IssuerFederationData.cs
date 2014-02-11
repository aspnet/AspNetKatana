// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.WsFederation
{
    /// <summary>
    /// Signing metadata parsed from a WSFed endpoint.
    /// </summary>
    internal class IssuerFederationData
    {
        /// <summary>
        /// Gets or sets the Signing tokens.
        /// </summary>
        public IEnumerable<X509SecurityToken> IssuerSigningTokens { get; set; }

        /// <summary>
        /// Gets or sets the passive token endpoint.
        /// </summary>
        public string PassiveTokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the token issuer name.
        /// </summary>
        public string TokenIssuerName { get; set; }
    }
}
