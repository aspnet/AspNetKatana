// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Microsoft.Owin.Security.Notifications
{
    /// <summary>
    /// This Notification can be used to be informed when an 'AuthorizationCode' is received over the OpenIdConnect protocol.
    /// </summary>
    public class AuthorizationCodeReceivedNotification : BaseNotification<OpenIdConnectAuthenticationOptions>
    {
        /// <summary>
        /// Creates a <see cref="AuthorizationCodeReceivedNotification"/>
        /// </summary>
        public AuthorizationCodeReceivedNotification(IOwinContext context, OpenIdConnectAuthenticationOptions options)
            : base(context, options)
        { 
        }

        /// <summary>
        /// Gets or sets the <see cref="AuthenticationTicket"/>
        /// </summary>
        public AuthenticationTicket AuthenticationTicket { get; set; }

        /// <summary>
        /// Gets or sets the 'code'.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JwtSecurityToken"/> that was received in the id_token + code OpenIdConnectRequest.
        /// </summary>
        public JwtSecurityToken JwtSecurityToken { get; set; }

        /// <summary>
        /// The request that will be sent to the token endpoint and is available for customization.
        /// </summary>
        public OpenIdConnectMessage TokenEndpointRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OpenIdConnectMessage"/>.
        /// </summary>
        public OpenIdConnectMessage ProtocolMessage { get; set; }

        /// <summary>
        /// Gets or sets the 'redirect_uri'.
        /// </summary>
        /// <remarks>This is the redirect_uri that was sent in the id_token + code OpenIdConnectRequest.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "user controlled, not necessarily a URI")]
        public string RedirectUri { get; set; }

        /// <summary>
        /// If the developer chooses to redeem the code themselves then they can provide the resulting tokens here. This is the
        /// same as calling HandleCodeRedemption. If set then the handler will not attempt to redeem the code. An IdToken
        /// is required if one had not been previously received in the authorization response.
        /// </summary>
        public OpenIdConnectMessage TokenEndpointResponse { get; set; }

        /// <summary>
        /// Indicates if the developer choose to handle (or skip) the code redemption. If true then the handler will not attempt
        /// to redeem the code. See HandleCodeRedemption and TokenEndpointResponse.
        /// </summary>
        public bool HandledCodeRedemption
        {
            get
            {
                return TokenEndpointResponse != null;
            }
        }

        /// <summary>
        /// Tells the handler to skip the code redemption process. The developer may have redeemed the code themselves, or
        /// decided that the redemption was not required. If tokens were retrieved that are needed for further processing then
        /// call one of the overloads that allows providing tokens. An IdToken is required if one had not been previously received
        /// in the authorization response. Calling this is the same as setting TokenEndpointResponse.
        /// </summary>
        public void HandleCodeRedemption()
        {
            TokenEndpointResponse = new OpenIdConnectMessage();
        }

        /// <summary>
        /// Tells the handler to skip the code redemption process. The developer may have redeemed the code themselves, or
        /// decided that the redemption was not required. If tokens were retrieved that are needed for further processing then
        /// call one of the overloads that allows providing tokens. An IdToken is required if one had not been previously received
        /// in the authorization response. Calling this is the same as setting TokenEndpointResponse.
        /// </summary>
        public void HandleCodeRedemption(string accessToken, string idToken)
        {
            TokenEndpointResponse = new OpenIdConnectMessage() { AccessToken = accessToken, IdToken = idToken };
        }

        /// <summary>
        /// Tells the handler to skip the code redemption process. The developer may have redeemed the code themselves, or
        /// decided that the redemption was not required. If tokens were retrieved that are needed for further processing then
        /// call one of the overloads that allows providing tokens. An IdToken is required if one had not been previously received
        /// in the authorization response. Calling this is the same as setting TokenEndpointResponse.
        /// </summary>
        public void HandleCodeRedemption(OpenIdConnectMessage tokenEndpointResponse)
        {
            TokenEndpointResponse = tokenEndpointResponse;
        }
    }
}