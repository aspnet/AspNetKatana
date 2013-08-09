// <copyright file="ActiveDirectoryFederationServicesOptions.cs" company="Microsoft Open Technologies, Inc.">
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

using Microsoft.Owin.Security.OAuth;

namespace Microsoft.Owin.Security.ActiveDirectory
{
    /// <summary>
    /// Options to configure the Active Directory Federation Services JWT middleware.
    /// </summary>
    public class ActiveDirectoryFederationServicesBearerAuthenticationOptions : AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryFederationServicesBearerAuthenticationOptions"/> class.
        /// </summary>
        public ActiveDirectoryFederationServicesBearerAuthenticationOptions() : base("Bearer")
        {
            ValidateMetadataEndpointCertificate = true;
        }

        /// <summary>
        /// Gets or sets the WSFed Metadata Endpoint for the Security Token Service JWTs will be issued from.
        /// </summary>
        /// <value>
        /// The WSFed Metadata Endpoint for the Security Token Service JWTs will be issued from.
        /// </value>
        public string MetadataEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the expected audience for any received JWT token.
        /// </summary>
        /// <value>
        /// The expected audience for any received JWT token.
        /// </value>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the authentication realm.
        /// </summary>
        /// <value>
        /// The authentication realm.
        /// </value>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the authentication provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IOAuthBearerAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the HTTPS certificate on the metadata endpoint should be validated.
        /// </summary>
        /// <value>
        /// true if the metadata endpoint certificate should be validated, otherwise false.
        /// </value>
        public bool ValidateMetadataEndpointCertificate { get; set; }
    }
}
