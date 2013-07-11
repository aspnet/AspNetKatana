// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateResourceOwnerCredentialsContext : BaseContext
    {
        public OAuthValidateResourceOwnerCredentialsContext(
            IOwinContext context,
            string clientId,
            string userName,
            string password,
            string scope) : base(context)
        {
            ClientId = clientId;
            UserName = userName;
            Password = password;
            Scope = scope;
        }

        public string ClientId { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string Scope { get; private set; }

        public ClaimsIdentity Identity { get; private set; }
        public AuthenticationExtra Extra { get; private set; }

        public bool IsValidated { get; private set; }

        public void Validated(ClaimsIdentity identity, AuthenticationExtra extra)
        {
            Identity = identity;
            Extra = extra;
            IsValidated = true;
        }
    }
}
