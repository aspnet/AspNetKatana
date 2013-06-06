// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsResponseSignInContext
    {
        public FormsResponseSignInContext(IDictionary<string, object> environment, string authenticationType, ClaimsIdentity identity, AuthenticationExtra extra)
        {
            Environment = environment;
            AuthenticationType = authenticationType;
            Identity = identity;
            Extra = extra;
        }

        public IDictionary<string, object> Environment { get; private set; }
        public string AuthenticationType { get; private set; }

        public ClaimsIdentity Identity { get; set; }
        public AuthenticationExtra Extra { get; set; }
    }
}
