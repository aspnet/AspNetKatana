// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Owin.Security.OAuth.Messages;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Provides context information when processing an Authorization Response
    /// </summary>
    public class OAuthAuthorizationEndpointResponseContext : EndpointContext<OAuthAuthorizationServerOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthAuthorizationEndpointResponseContext"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="ticket"></param>
        /// <param name="tokenEndpointRequest"></param>
        public OAuthAuthorizationEndpointResponseContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            AuthenticationTicket ticket,
            AuthorizeEndpointRequest authorizeEndpointRequest,
            string accessToken,
            string authorizationCode)
            : base(context, options)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            Identity = ticket.Identity;
            Properties = ticket.Properties;
            AuthorizeEndpointRequest = authorizeEndpointRequest;
            AdditionalResponseParameters = new Dictionary<string, object>(StringComparer.Ordinal);
            AccessToken = accessToken;
            AuthorizationCode = authorizationCode;
        }

        /// <summary>
        /// Gets the identity of the resource owner.
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// Dictionary containing the state of the authentication session.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }

        /// <summary>
        /// Gets information about the authorize endpoint request. 
        /// </summary>
        public AuthorizeEndpointRequest AuthorizeEndpointRequest { get; private set; }

        /// <summary>
        /// Enables additional values to be appended to the token response.
        /// </summary>
        public IDictionary<string, object> AdditionalResponseParameters { get; private set; }

        /// <summary>
        /// The serialized Access-Token. Depending on the flow, it can be null.
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// The created Authorization-Code. Depending on the flow, it can be null.
        /// </summary>
        public string AuthorizationCode { get; private set; }
    }
}
