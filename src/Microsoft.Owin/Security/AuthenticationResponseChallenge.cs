using System.Collections.Generic;

namespace Microsoft.Owin.Security
{
    public class AuthenticationResponseChallenge
    {
        public AuthenticationResponseChallenge(string[] authenticationTypes, IDictionary<string, string> extra)
        {
            AuthenticationTypes = authenticationTypes;
            Extra = extra;
        }

        public string[] AuthenticationTypes { get; private set; }
        public IDictionary<string, string> Extra { get; private set; }
    }
}
