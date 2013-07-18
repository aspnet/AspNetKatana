// <copyright file="ISigningSecurityTokenProvider.cs" company="Microsoft Open Technologies, Inc.">
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

using System.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Provides signing properties to the implementing class.
    /// </summary>
    public interface ISigningSecurityTokenProvider : ISecurityTokenProvider
    {
        /// <summary>
        /// Gets the JWT issuer.
        /// </summary>
        /// <value>
        /// The JWT issuer.
        /// </value>
        string Issuer
        {
            get;
        }

        /// <summary>
        /// Gets the credentials used to sign the JWT.
        /// </summary>
        /// <value>
        /// The credentials used to sign the JWT.
        /// </value>
        SigningCredentials SigningCredentials
        {
            get;
        }
    }
}
