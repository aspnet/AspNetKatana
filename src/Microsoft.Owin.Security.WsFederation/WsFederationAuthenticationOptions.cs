// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens;
using System.Net.Http;
using Microsoft.IdentityModel.Extensions;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.Owin.Security.WsFederation
{
    /// <summary>
    /// Configuration options for <see cref="WsFederationAuthenticationMiddleware"/>
    /// </summary>
    public class WsFederationAuthenticationOptions : AuthenticationOptions
    {
        private SecurityTokenHandlerCollection _securityTokenHandlers;
        private TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// Initializes a new <see cref="WsFederationAuthenticationOptions"/>
        /// </summary>
        public WsFederationAuthenticationOptions()
            : this(WsFederationAuthenticationDefaults.AuthenticationType)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="WsFederationAuthenticationOptions"/>
        /// </summary>
        /// <param name="authenticationType"> corresponds to the IIdentity AuthenticationType property. <see cref="AuthenticationOptions.AuthenticationType"/>.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "WsFederation", Justification = "Not a LOC field")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Microsoft.Owin.Security.WsFederation.WsFederationAuthenticationOptions.set_Caption(System.String)", Justification = "Not a LOC field")]
        public WsFederationAuthenticationOptions(string authenticationType)
            : base(authenticationType)
        {
            AuthenticationMode = Security.AuthenticationMode.Active;
            Caption = WsFederationAuthenticationDefaults.Caption;
            _tokenValidationParameters = new TokenValidationParameters();
            BackchannelTimeout = TimeSpan.FromMinutes(1);
            UseTokenLifetime = true;
            RefreshOnIssuerKeyNotFound = true;
        }

        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// when retrieving metadata.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to retrieve metadata.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value
        /// is a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications.
        /// </summary>
        /// <value>
        /// The back channel timeout in milliseconds.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        /// <summary>
        /// Configuration provided directly by the developer. If provided, then MetadataAddress and the Backchannel properties
        /// will not be used. This information should not be updated during request processing.
        /// </summary>
        public WsFederationConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the address to retrieve the wsFederation metadata
        /// </summary>
        public string MetadataAddress { get; set; }

        /// <summary>
        /// Responsible for retrieving, caching, and refreshing the configuration from metadata.
        /// If not provided, then one will be created using the MetadataAddress and Backchannel properties.
        /// </summary>
        public IConfigurationManager<WsFederationConfiguration> ConfigurationManager { get; set; }

        /// <summary>
        /// Gets or sets if a metadata refresh should be attempted after a SecurityTokenSignatureKeyNotFoundException. This allows for automatic
        /// recovery in the event of a signature key rollover. This is enabled by default.
        /// </summary>
        public bool RefreshOnIssuerKeyNotFound { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WsFederationAuthenticationNotifications"/> to call when processing WsFederation messages.
        /// </summary>
        public WsFederationAuthenticationNotifications Notifications { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenHandlerCollection"/> of <see cref="SecurityTokenHandler"/>s used to read and validate <see cref="SecurityToken"/>s.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "By design")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "By design")]
        public SecurityTokenHandlerCollection SecurityTokenHandlers
        {
            get
            {
                return _securityTokenHandlers;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("SecurityTokenHandlers");
                }

                _securityTokenHandlers = value;
            }
        }

        /// <summary>
        /// Gets or sets the AuthenticationType used when creating the <see cref="System.Security.Claims.ClaimsIdentity"/>.
        /// </summary>
        public string SignInAsAuthenticationType
        {
            get { return TokenValidationParameters.AuthenticationType; }
            set { TokenValidationParameters.AuthenticationType = value; }
        }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TokenValidationParameters"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"> if 'TokenValidationParameters' is null.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Name of the property.")]
        public TokenValidationParameters TokenValidationParameters
        {
            get
            {
                return _tokenValidationParameters;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("TokenValidationParameters");
                }

                _tokenValidationParameters = value;
            }
        }

        /// <summary>
        /// Gets or sets the 'wreply'.
        /// </summary>
        public string Wreply { get; set; }

        /// <summary>
        /// Gets or sets the 'wreply' value used during sign-out.
        /// If none is specified then the value from the Wreply field is used.
        /// </summary>
        public string SignOutWreply { get; set; }
        
        /// <summary>
        /// Gets or sets the 'wtrealm'.
        /// </summary>
        public string Wtrealm { get; set; }

        /// <summary>
        /// An optional constrained path on which to process the authentication callback. Computed from Wreply if not provided.
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// Indicates that the authentication session lifetime (e.g. cookies) should match that of the authentication token.
        /// If the token does not provide lifetime information then normal session lifetimes will be used.
        /// This is enabled by default.
        /// </summary>
        public bool UseTokenLifetime
        {
            get;
            set;
        }
    }
}