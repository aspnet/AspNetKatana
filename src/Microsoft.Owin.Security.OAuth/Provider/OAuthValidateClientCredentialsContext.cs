// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateClientCredentialsContext : BaseContext
    {
        public OAuthValidateClientCredentialsContext(
            IOwinContext context,
            string clientId,
            string scope) : base(context)
        {
            ClientId = clientId;
            Scope = scope;
        }

        public string ClientId { get; private set; }
        public string Scope { get; private set; }

        public ClaimsIdentity Identity { get; private set; }
        public IDictionary<string, string> Extra { get; private set; }

        public bool IsValidated { get; private set; }

        public void Validated(ClaimsIdentity identity, IDictionary<string, string> extra)
        {
            Identity = identity;
            Extra = extra;
            IsValidated = true;
        }
    }
}
