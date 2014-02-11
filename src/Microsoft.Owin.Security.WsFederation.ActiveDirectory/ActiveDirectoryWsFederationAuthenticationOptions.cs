// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.WsFederation.ActiveDirectory
{
    /// <summary>
    /// Configuration options for ActiveDirectory using WsFederation.
    /// </summary>
    public class ActiveDirectoryWsFederationAuthenticationOptions : WsFederationAuthenticationOptions
    {
        /// <summary>
        /// Initializes a new <see cref="ActiveDirectoryWsFederationAuthenticationOptions"/>
        /// </summary>
        public ActiveDirectoryWsFederationAuthenticationOptions()
            : base(authenticationType: WsFederationAuthenticationDefaults.AuthenticationType)
        {
            AuthenticationMode = AuthenticationMode.Active;
        }

        /// <summary>
        /// Gets or sets the ActiveDirectory tenant.
        /// </summary>
        public string Tenant { get; set; }
    }
}