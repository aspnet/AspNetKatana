// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Xml.Linq;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Google
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class GoogleAuthenticatedContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="GoogleAuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> representing the user</param>
        /// <param name="properties">A property bag for common authentication properties</param>
        /// <param name="responseMessage"></param>
        /// <param name="attributeExchangeProperties"></param>
        public GoogleAuthenticatedContext(
            IOwinContext context,
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            XElement responseMessage,
            IDictionary<string, string> attributeExchangeProperties)
            : base(context)
        {
            Identity = identity;
            Properties = properties;
            ResponseMessage = responseMessage;
            AttributeExchangeProperties = attributeExchangeProperties;
        }

        /// <summary>
        /// Gets or sets the <see cref="ClaimsIdentity"/> representing the user
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }

        /// <summary>
        /// Gets or sets parsed response message from openid query string
        /// </summary>
        public XElement ResponseMessage { get; set; }

        /// <summary>
        /// Gets the key-value dictinary of message properties
        /// </summary>
        public IDictionary<string, string> AttributeExchangeProperties { get; private set; }
    }
}
