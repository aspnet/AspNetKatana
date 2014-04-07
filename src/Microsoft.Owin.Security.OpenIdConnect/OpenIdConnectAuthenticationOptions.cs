// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Extensions;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// Configuration options for <see cref="OpenIdConnectAuthenticationOptions"/>
    /// </summary>
    public class OpenIdConnectAuthenticationOptions : AuthenticationOptions
    {
        private TimeSpan _timeSpan = TimeSpan.FromHours(1);
        private SecurityTokenHandlerCollection _securityTokenHandlers;
        private TokenValidationParameters _tokenValidationParameters;

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
        /// <param name="authenticationType"> will be used to when creating the <see cref="ClaimsIdentity"/> for the AuthenticationType property.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions.set_Caption(System.String)", Justification = "Not a LOC field")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OpenIdConnect", Justification = "It is correct.")]
        public OpenIdConnectAuthenticationOptions(string authenticationType)
            : base(authenticationType)
        {
            AuthenticationMode = Security.AuthenticationMode.Active;
            Caption = OpenIdConnectAuthenticationDefaults.Caption;
            BackchannelTimeout = TimeSpan.FromMinutes(1);
            Response_Mode = "form_post";
            Response_Type = OpenIdConnectResponseTypes.Code_Id_Token;
            Scope = OpenIdConnectScopes.OpenId_Profile;
            TokenValidationParameters = new TokenValidationParameters();
        }

        /// <summary>
        /// Gets or sets the Authority to use when making OpenIdConnect calls.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// An optional constrained path on which to process the authentication callback.
        /// </summary>
        public PathString AuthorizeCallback { get; set; }

        /// <summary>
        /// Gets or sets the AuthorizeEndpoint
        /// </summary>
        /// <remarks>This endpoint is used for obtaining the id_token, code and accesssTokens</remarks>
        public string Authorization_Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ICertificateValidator"/> used to validate the certificate protecting the <see cref="BackchannelHttpHander"/>.
        /// </summary>
        public ICertificateValidator BackchannelCertificateValidator { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpMessageHandler"/> to use for making server to server calls.
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
                return _timeSpan;
            }

            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException("BackchannelTimeout", value, Resources.ArgsException_BackchallelLessThanZero);
                }

                _timeSpan = value;
            }
        }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        /// <summary>
        /// Gets or sets the 'end_session_endpoint'
        /// </summary>
        /// <remarks>This endpoint is used for ending the session for user.</remarks>
        public string End_Session_Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SecurityTokenHandlerCollection"/> of <see cref="SecurityTokenHandler"/>s used to read and validate <see cref="SecurityToken"/>s. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "By Design")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "By Design")]
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
        /// Gets or sets the 'client_id'.
        /// </summary>
        public string Client_Id { get; set; }

        /// <summary>
        /// Gets or sets the 'client_secret'.
        /// </summary>
        public string Client_Secret { get; set; }

        /// <summary>
        /// Gets or sets the discovery endpoint for obtaining metadata
        /// </summary>
        public string MetadataAddress { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OpenIdConnectAuthenticationNotifications"/> to notify when processing OpenIdConnect messages.
        /// </summary>
        public OpenIdConnectAuthenticationNotifications Notifications { get; set; }

        /// <summary>
        /// Gets or sets the 'post_logout_redirect_uri'
        /// </summary>
        /// <remarks>This is sent to the OP as the redirect for the user-agent.</remarks>
        public string Post_Logout_Redirect_Uri { get; set; }

        /// <summary>
        /// Gets or sets the 'redirect_uri'.
        /// </summary>
        public string Redirect_Uri { get; set; }

        /// <summary>
        /// Gets or sets the 'resource'.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets the 'response_mode'.
        /// </summary>
        public string Response_Mode { get; set; }

        /// <summary>
        /// Gets or sets the 'response_type'.
        /// </summary>
        public string Response_Type { get; set; }

        /// <summary>
        /// Gets or sets the 'scope'.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the name of the authentication type that will be assigned to the <see cref="System.Security.Claims.ClaimsIdentity"/> representing the signed in user.
        /// </summary>
        public string SignInAsAuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
                
        /// <summary>
        /// Gets or sets the 'token_endpoint'
        /// </summary>
        public string Token_Endpoint { get; set; }

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
    }
}