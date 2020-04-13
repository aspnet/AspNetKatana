// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Provides context information used in handling an OAuth resource owner grant.
    /// </summary>
    public class OAuthGrantResourceOwnerCredentialsContext : BaseValidatingTicketContext<OAuthAuthorizationServerOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthGrantResourceOwnerCredentialsContext"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="clientId"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="scope"></param>
        public OAuthGrantResourceOwnerCredentialsContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            string clientId,
            string username,
            string password,
            IList<string> scope)
            : base(context, options, null)
        {
            ClientId = clientId;
            Username = username;
            Password = password;
            Scope = scope;
        }

        /// <summary>
        /// OAuth client id.
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Resource owner username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Resource owner password.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// List of scopes allowed by the resource owner.
        /// </summary>
        public IList<string> Scope { get; private set; }
    }
}
