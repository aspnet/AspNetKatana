// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// Configuration options for <see cref="OpenIdConnectAuthenticationOptions"/>
    /// </summary>
    public class OpenIdConnectAuthenticationOptions : AuthenticationOptions
    {
        private OpenIdConnectProtocolValidator _protocolValidator;
        private TokenValidationParameters _tokenValidationParameters;
        private TimeSpan _backchannelTimeout;
        private ISecurityTokenValidator _securityTokenValidator;

        /// <summary>
        /// Initializes a new <see cref="OpenIdConnectAuthenticationOptions"/>
        /// </summary>
        public OpenIdConnectAuthenticationOptions()
            : this(OpenIdConnectAuthenticationDefaults.AuthenticationType)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="OpenIdConnectAuthenticationOptions"/>
        /// </summary>
        /// <remarks>
        /// Defaults:
        /// <para>AddNonceToRequest: true.</para>
        /// <para>AuthenticationMode: <see cref="AuthenticationMode.Active"/>.</para>
        /// <para>BackchannelTimeout: 1 minute.</para>
        /// <para>Caption: <see cref="OpenIdConnectAuthenticationDefaults.Caption"/>.</para>
        /// <para>ProtocolValidator: new <see cref="OpenIdConnectProtocolValidator"/>.</para>
        /// <para>RefreshOnIssuerKeyNotFound: true</para>
        /// <para>ResponseMode: <see cref="OpenIdConnectResponseMode.FormPost"/></para>
        /// <para>ResponseType: <see cref="OpenIdConnectResponseTypes.CodeIdToken"/></para>
        /// <para>Scope: <see cref="OpenIdConnectScopes.OpenIdProfile"/>.</para>
        /// <para>TokenValidationParameters: new <see cref="TokenValidationParameters"/> with AuthenticationType = authenticationType.</para>
        /// <para>UseTokenLifetime: true.</para>
        /// <para>RedeemCode: false.</para>
        /// </remarks>
        /// <param name="authenticationType"> will be used to when creating the <see cref="System.Security.Claims.ClaimsIdentity"/> for the AuthenticationType property.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions.set_Caption(System.String)", Justification = "Not a LOC field")]
        public OpenIdConnectAuthenticationOptions(string authenticationType)
            : base(authenticationType)
        {
            AuthenticationMode = Security.AuthenticationMode.Active;
            BackchannelTimeout = TimeSpan.FromMinutes(1);
            Caption = OpenIdConnectAuthenticationDefaults.Caption;
            ProtocolValidator = new OpenIdConnectProtocolValidator()
            {
                RequireStateValidation = false,
                NonceLifetime = TimeSpan.FromMinutes(15)
            };
            RefreshOnIssuerKeyNotFound = true;
            ResponseMode = OpenIdConnectResponseMode.FormPost;
            ResponseType = OpenIdConnectResponseType.CodeIdToken;
            Scope = OpenIdConnectScope.OpenIdProfile;
            SecurityTokenValidator = new JwtSecurityTokenHandler();
            RequireHttpsMetadata = true;
            TokenValidationParameters = new TokenValidationParameters();
            UseTokenLifetime = true;
            CookieManager = new CookieManager();
            RedeemCode = false;
        }

        /// <summary>
        /// Gets or sets the Authority to use when making OpenIdConnect calls.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// An optional constrained path on which to process the authentication callback.
        /// If not provided and RedirectUri is available, this value will be generated from RedirectUri.
        /// </summary>
        /// <remarks>If you set this value, then the <see cref="OpenIdConnectAuthenticationHandler"/> will only listen for posts at this address. 
        /// If the IdentityProvider does not post to this address, you may end up in a 401 -> IdentityProvider -> Client -> 401 -> ...</remarks>
        public PathString CallbackPath { get; set; }

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
        /// Gets or sets the timeout when using the backchannel to make an http call.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "By design we use the property name in the exception")]
        public TimeSpan BackchannelTimeout
        {
            get
            {
                return _backchannelTimeout;
            }

            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("BackchannelTimeout", value, Resources.ArgsException_BackchallelLessThanZero);
                }

                _backchannelTimeout = value;
            }
        }

        /// <summary>
        /// Used to communicate with the remote identity provider.
        /// </summary>
        public HttpClient Backchannel { get; set; }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        /// <summary>
        /// Gets or sets the 'client_id'.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the 'client_secret'.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Configuration provided directly by the developer. If provided, then MetadataAddress and the Backchannel properties
        /// will not be used. This information should not be updated during request processing.
        /// </summary>
        public OpenIdConnectConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets if HTTPS is required for the metadata address or authority.
        /// The default is true. This should be disabled only in development environments.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; }

        /// <summary>
        /// Gets or sets the discovery endpoint for obtaining metadata
        /// </summary>
        public string MetadataAddress { get; set; }

        /// <summary>
        /// Responsible for retrieving, caching, and refreshing the configuration from metadata.
        /// If not provided, then one will be created using the MetadataAddress and Backchannel properties.
        /// </summary>
        public IConfigurationManager<OpenIdConnectConfiguration> ConfigurationManager { get; set; }

        /// <summary>
        /// Gets or sets if a metadata refresh should be attempted after a SecurityTokenSignatureKeyNotFoundException. This allows for automatic
        /// recovery in the event of a signature key rollover. This is enabled by default.
        /// </summary>
        public bool RefreshOnIssuerKeyNotFound { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OpenIdConnectAuthenticationNotifications"/> to notify when processing OpenIdConnect messages.
        /// </summary>
        public OpenIdConnectAuthenticationNotifications Notifications { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OpenIdConnectProtocolValidator"/> that is used ensure the 'id_token' received
        /// is valid per: http://openid.net/specs/openid-connect-core-1_0.html#IDTokenValidation 
        /// </summary>
        /// <exception cref="ArgumentNullException">if 'value' is null.</exception>
        public OpenIdConnectProtocolValidator ProtocolValidator
        {
            get
            {
                return _protocolValidator;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _protocolValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets the 'post_logout_redirect_uri'
        /// </summary>
        /// <remarks>This is sent to the OP as the redirect for the user-agent.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout", Justification = "This is the term used in the spec.")]
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the 'redirect_uri'.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By Design")]
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the 'resource'.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets the 'response_mode'.
        /// </summary>
        public string ResponseMode { get; set; }

        /// <summary>
        /// Gets or sets the 'response_type'.
        /// </summary>
        public string ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the 'scope'.
        /// </summary>
        public string Scope { get; set; }

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
        /// Gets or sets the <see cref="ISecurityTokenValidator"/> used to validate identity tokens.
        /// </summary>
        public ISecurityTokenValidator SecurityTokenValidator
        {
            get { return _securityTokenValidator; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _securityTokenValidator = value;
            }
        }

        /// <summary>
        /// Gets or sets the TokenValidationParameters
        /// </summary>
        /// <remarks>Contains the types and definitions required for validating a token.</remarks>
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
                    throw new ArgumentNullException("value");
                }

                _tokenValidationParameters = value;
            }
        }

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

        /// <summary>
        /// Defines whether access and refresh tokens should be stored in the
        /// <see cref="AuthenticationProperties"/> after a successful authorization.
        /// This property is set to <c>false</c> by default to reduce
        /// the size of the final authentication cookie.
        /// </summary>
        public bool SaveTokens { get; set; }

        /// <summary>
        /// An abstraction for reading and setting cookies during the authentication process.
        /// </summary>
        public ICookieManager CookieManager { get; set; }

        /// <summary>
        /// When set to <c>true</c> the authorization code will be redeemed for tokens at the token endpoint.
        /// This property is set to <c>false</c> by default.
        /// </summary>
        public bool RedeemCode { get; set; }
    }
}