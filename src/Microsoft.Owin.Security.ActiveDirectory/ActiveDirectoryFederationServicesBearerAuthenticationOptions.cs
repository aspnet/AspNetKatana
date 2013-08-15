// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
