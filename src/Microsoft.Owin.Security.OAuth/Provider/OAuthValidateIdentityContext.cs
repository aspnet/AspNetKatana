// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateIdentityContext
    {
        public OAuthValidateIdentityContext(ClaimsIdentity identity, IDictionary<string, string> extra)
        {
            Identity = identity;
            Extra = extra;
        }

        public ClaimsIdentity Identity { get; private set; }
        public IDictionary<string, string> Extra { get; private set; }

        public void ReplaceIdentity(ClaimsIdentity identity)
        {
            Identity = identity;
        }

        public void RejectIdentity()
        {
            Identity = null;
        }
    }
}
