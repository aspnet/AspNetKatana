// <copyright file="IIssuerSecurityTokenProvider.cs" company="Microsoft Open Technologies, Inc.">
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
    public interface IIssuerSecurityTokenProvider
    {
        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        string Issuer
        {
            get;
        }

        /// <summary>
        /// Gets the expected security token for the specified <paramref name="identifier"/> for use in signature validation.
        /// </summary>
        /// <param name="identifier">The token identifier.</param>
        /// <returns>The security token identified by <paramref name="identifier"/>.</returns>
        SecurityToken GetSecurityTokenForKeyIdentifier(string identifier);

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <returns>All known security tokens.</returns>
        IEnumerable<SecurityToken> GetSecurityTokens();
    }
}
