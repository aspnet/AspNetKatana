// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IdentityModel.Tokens;
using System.Net.Http;

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
            BackchannelTimeout = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// Gets or sets the expected audience for any received JWT token.
        /// </summary>
        /// <value>
        /// The expected audience for any received JWT token.
        /// </value>
        [Obsolete("Use TokenValidationParameters.ValidAudience", error: false)]
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
        /// Gets or sets the address to retrieve the configuration metadata
        /// This can be generated from the Tenant if present.
        /// </summary>
        public string MetadataAddress { get; set; }

        /// <summary>
        /// Gets or sets the authentication provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IOAuthBearerAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the a certificate validator to use to validate the metadata endpoint.
        /// </summary>
        /// <value>
        /// The certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        /// Gets or sets timeout value in for back channel communications with the metadata endpoint.
        /// </summary>
        /// <value>
        /// The back channel timeout.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with the metadata endpoint.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TokenValidationParameters"/> used to determine if a token is valid.
        /// </summary>
        public TokenValidationParameters TokenValidationParameters { get; set; }

        /// <summary>
        /// A System.IdentityModel.Tokens.SecurityTokenHandler designed for creating and validating Json Web Tokens.
        /// </summary>
        public JwtSecurityTokenHandler TokenHandler { get; set; }
    }
}
