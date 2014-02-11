// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.WsFederation.ActiveDirectory
{
    /// <summary>
    /// Endpoint values for Active Directory
    /// </summary>
    public static class ActiveDirectoryWsFederationEndpoints
    {
        /// <summary>
        /// Active Directory endpoint for issuing token.
        /// </summary>
        public const string IssuerAddress = "https://login.windows.net";

        /// <summary>
        /// Active Directory issuer name found inside tokens.
        /// </summary>
        public const string IssuerName = "https://sts.windows.net";

        /// <summary>
        /// Active Directory federation token endpoint.
        /// </summary>
        public const string WsFed = "wsfed";

        /// <summary>
        /// Active Directory federation metadata endpoint.
        /// </summary>       
        public const string WsFedMetadata = "FederationMetadata/2007-06/FederationMetadata.xml";
    }
}