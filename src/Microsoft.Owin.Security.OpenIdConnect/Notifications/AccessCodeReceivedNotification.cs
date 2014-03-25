//-----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

namespace Microsoft.Owin.Security.Notifications
{
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens;
    using System.Security.Claims;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.Owin.Security.OpenIdConnect;

    /// <summary>
    /// This Notification can be used to be informed when an 'AccessCode' is received over the OpenIdConnect protocol.
    /// </summary>
    public class AccessCodeReceivedNotification : BaseNotification<OpenIdConnectAuthenticationOptions>
    {
        /// <summary>
        /// Creates a <see cref="AccessCodeReceivedNotification"/>
        /// </summary>
        public AccessCodeReceivedNotification(IOwinContext context, OpenIdConnectAuthenticationOptions options)
            : base(context, options)
        { 
        }
        
        /// <summary>
        /// Gets or sets the 'code'.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClaimsIdentity"/> that was received in the id_token + code OpenIdConnectRequest.
        /// </summary>
        public ClaimsIdentity ClaimsIdentity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="JwtSecurityToken"/> that was received in the id_token + code OpenIdConnectRequest.
        /// </summary>
        public JwtSecurityToken JwtSecurityToken { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OpenIdConnectMessage"/>.
        /// </summary>
        public OpenIdConnectMessage ProtocolMessage { get; set; }

        /// <summary>
        /// Gets or sets the 'redirect_uri'.
        /// </summary>
        /// <remarks>This is the redirect_uri that was sent in the id_token + code OpenIdConnectRequest.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "by design using spec values")]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "user controlled, not necessarily a URI")]
        public string Redirect_Uri { get; set; }
    }
}