// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsValidateLoginContext
    {
        public FormsValidateLoginContext(IDictionary<string, object> environment, string authenticationType, string name, string password)
        {
            Environment = environment;
            AuthenticationType = authenticationType;
        }

        public IDictionary<string, object> Environment { get; private set; }
        public string AuthenticationType { get; private set; }

        public IIdentity Identity { get; private set; }

        public void SignIn(IIdentity identity)
        {
            Identity = identity;
        }

        public void SignIn(string name, params Claim[] claims)
        {
            SignIn(name, (IEnumerable<Claim>)claims);
        }

        public void SignIn(string name, IEnumerable<Claim> claims)
        {
            Identity = new ClaimsIdentity(new GenericIdentity(name, AuthenticationType), claims);
        }
    }
}
