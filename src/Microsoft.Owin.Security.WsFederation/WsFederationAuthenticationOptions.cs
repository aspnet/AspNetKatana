// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens;
using System.Net.Http;
using Microsoft.IdentityModel.Extensions;

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
            CallbackPath = new PathString("/signin-wsfed");

            _tokenValidationParameters = new TokenValidationParameters();
            BackchannelTimeout = TimeSpan.FromMinutes(1);
        }

        public ICertificateValidator BackchannelCertificateValidator { get; set; }
        public HttpMessageHandler BackchannelHttpHandler { get; set; }
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
        /// Gets or sets the address of the issuer.
        /// </summary>
        public string IssuerAddress { get; set; }

        /// <summary>
        /// Gets or sets the address to retrieve the wsFederation metadata
        /// </summary>
        public string MetadataAddress { get; set; }

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
                    throw new ArgumentNullException("SecurityTokenHandlerCollection");
                }

                _securityTokenHandlers = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of another authentication middleware which will be responsible for actually 
        /// issuing a user <see cref="System.Security.Claims.ClaimsIdentity"/>.
        /// </summary>
        public string SignInAsAuthenticationType { get; set; }

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
        /// Gets or sets the 'wtrealm'.
        /// </summary>
        public string Wtrealm { get; set; }

        /// <summary>
        /// An optional constrained path on which to process the authentication callback.
        /// </summary>
        public PathString CallbackPath { get; set; }
    }
}