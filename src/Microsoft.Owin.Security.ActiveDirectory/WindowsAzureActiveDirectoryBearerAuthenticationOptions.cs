// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.OAuth;

namespace Microsoft.Owin.Security.ActiveDirectory
{
    /// <summary>
    /// Options to configure the Windows Azure Active Directory JWT middleware.
    /// </summary>
    public class WindowsAzureActiveDirectoryBearerAuthenticationOptions : AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsAzureActiveDirectoryBearerAuthenticationOptions"/> class.
        /// </summary>
        public WindowsAzureActiveDirectoryBearerAuthenticationOptions() : base("Bearer")
        {
            ValidateMetadataEndpointCertificate = true;
        }

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
        /// Gets or sets the Azure Active Directory tenant the tokens are issued from.
        /// </summary>
        /// <value>
        /// The Azure Active Directory tenant the tokens are issued from.
        /// </value>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the HTTPS certificate on the metadata endpoint should be validated.
        /// </summary>
        /// <value>
        /// true if the metadata endpoint certificate should be validated, otherwise false.
        /// </value>
        public bool ValidateMetadataEndpointCertificate { get; set; }

        /// <summary>
        /// Gets or sets the authentication provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IOAuthBearerAuthenticationProvider Provider { get; set; }
    }
}
