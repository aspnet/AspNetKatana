// <copyright file="ISecurityTokenProvider.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Jwt
{   
    /// <summary>
    /// Provides security token information to the implementing class.
    /// </summary>
    public interface ISecurityTokenProvider
    {
        /// <summary>
        /// Gets a value indicating whether the issuer of the JWT should be validated.
        /// </summary>
        /// <value>
        /// true if issuer should be validated; otherwise, false.
        /// </value>
        bool ValidateIssuer
        {
            get;
        }

        /// <summary>
        /// Gets the expected audiences for a JWT token.
        /// </summary>
        /// <value>
        /// The expected audiences for a JWT token.
        /// </value>
        IEnumerable<string> ExpectedAudiences
        {
            get;
        }

        /// <summary>
        /// Gets the expected security token for the specified <paramref name="identifier"/> for use in signature validation.
        /// </summary>
        /// <param name="identifier">The token identifier.</param>
        /// <returns>The security token identified by <paramref name="identifier"/>.</returns>
        SecurityToken GetSigningTokenForKeyIdentifier(string identifier);

        /// <summary>
        /// Gets the expected security token for the specified <paramref name="identifier"/> for use in signature validation.
        /// </summary>
        /// <param name="issuer">The issuer whose token to retrieve.</param>
        /// <param name="identifier">The token identifier.</param>
        /// <returns>The security token identified by <paramref name="identifier"/>.</returns>
        SecurityToken GetSigningTokenForKeyIdentifier(string issuer, string identifier);

        /// <summary>
        /// Gets the expected security tokens for the specified <paramref name="issuer"/> for use in signature validation.
        /// </summary>
        /// <param name="issuer">The issuer whose tokens to retrieve.</param>
        /// <returns>The known security tokens belonging to the specified <paramref name="issuer"/>.</returns>
        IEnumerable<SecurityToken> GetSigningTokensForIssuer(string issuer);

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <returns>All known security tokens.</returns>
        IEnumerable<SecurityToken> GetSigningTokens();
    }
}
