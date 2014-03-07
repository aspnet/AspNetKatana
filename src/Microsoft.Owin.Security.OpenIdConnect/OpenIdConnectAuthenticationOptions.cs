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
        public OpenIdConnectAuthenticationOptions(string authenticationType)
            : base(authenticationType)
        {
            AuthenticationMode = Security.AuthenticationMode.Active;
            AuthenticationType = authenticationType;
            Response_Mode = "form_post";
            Response_Type = OpenIdConnectResponseTypes.Id_Token;
            Scope = OpenIdConnectScopes.OpenId_Profile;
            SecurityTokenHandlers = SecurityTokenHandlerCollectionExtensions.GetDefaultHandlers(authenticationType);
            TokenValidationParameters = new TokenValidationParameters();
        }

        public ICertificateValidator BackchannelCertificateValidator { get; set; }
        public HttpMessageHandler BackchannelHttpHandler { get; set; }
        
        /// <summary>
        /// Gets or sets the timeout when using the backchannel to make an http call.
        /// </summary>
        public TimeSpan BackchannelTimeout 
        { 
            get 
            { 
                return _timeSpan; 
            } 
            
            set 
            { 
                _timeSpan = value; 
            } 
        }

        /// <summary>
        /// An optional constrained path on which to process the authentication callback.
        /// </summary>
        public PathString AuthorizeCallback { get; set; }

        /// <summary>
        /// Gets or sets the AuthorizeEndpoint
        /// </summary>
        /// <remarks>This endpoint is used for obtaining the id_token, code and accesssTokens</remarks>
        public string AuthorizeEndpoint { get; set; }

        /// <summary>
        /// Gets of sets the EndSessionEndpoint
        /// </summary>
        /// <remarks>This endpoint is used for ending the session for user.</remarks>
        public string EndSessionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the discovery endpoint for obtaining metadata
        /// </summary>
        public string MetadataAddress { get; set; }

        public SecurityTokenHandlerCollection SecurityTokenHandlers
        {
            get
            {
                return _securityTokenHandlers;
            }
            private set
            {
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
        /// Gets or sets the <see cref="OpenIdConnectAuthenticationNotifications"/> to notify when processing OpenIdConnect messages.
        /// </summary>
        public OpenIdConnectAuthenticationNotifications Notifications { get; set; }

        /// <summary>
        /// Gets or sets the 'redirect_uri'.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string Redirect_Uri { get; set; }

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
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the TokenEndpoint
        /// </summary>
        public string TokenEndpoint { get; set; }

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