// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Provides signing properties to the implementing class.
    /// </summary>
    public interface ISigningCredentialsProvider : IIssuerSecurityTokenProvider
    {
        /// <summary>
        /// Gets the credentials used to sign the JWT.
        /// </summary>
        /// <value>
        /// The credentials used to sign the JWT.
        /// </value>
        SigningCredentials SigningCredentials { get; }
    }
}
