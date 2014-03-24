// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IdentityModel.Tokens;

using Microsoft.Owin.Security.OAuth;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Options for JWT Bearer Token handler configuration.
    /// </summary>
    public class JwtBearerAuthenticationOptions : AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtBearerAuthenticationOptions"/> class.
        /// </summary>
        public JwtBearerAuthenticationOptions()
            : base(OAuthDefaults.AuthenticationType)
        {
        }

        /// <summary>
        /// Gets or sets the allowed audiences an inbound JWT will be checked against.
        /// </summary>
        /// <value>
        /// The allowed audiences.
        /// </value>
        public IEnumerable<string> AllowedAudiences { get; set; }

        /// <summary>
        /// Gets or sets the issuer security token providers which provide the signing keys
        /// a JWT signature is checked against.
        /// </summary>
        /// <value>
        /// The issuer security token providers.
        /// </value>
        public IEnumerable<IIssuerSecurityTokenProvider> IssuerSecurityTokenProviders { get; set; }

        /// <summary>
        /// Gets or sets the authentication provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IOAuthBearerAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the authentication realm.
        /// </summary>
        /// <value>
        /// The authentication realm.
        /// </value>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TokenValidationParameters"/> used to determine if a token is valid.
        /// </summary>
        public TokenValidationParameters TokenValidationParameters { get; set; }
    }
}
