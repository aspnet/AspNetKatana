using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Owin.Security
{
    public class AuthenticationResponseGrant
    {
        public AuthenticationResponseGrant(ClaimsIdentity identity, IDictionary<string, string> extra)
        {
            Principal = new ClaimsPrincipal(identity);
            Identity = identity;
            Extra = extra;
        }
        public AuthenticationResponseGrant(ClaimsPrincipal principal, IDictionary<string, string> extra)
        {
            Principal = principal;
            Identity = principal.Identities.FirstOrDefault();
            Extra = extra;
        }

        public ClaimsIdentity Identity { get; private set; }
        public ClaimsPrincipal Principal { get; private set; }
        public IDictionary<string, string> Extra { get; private set; }
    }
}
