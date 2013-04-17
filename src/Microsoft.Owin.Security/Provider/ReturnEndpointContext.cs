using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Provider
{
    public abstract class ReturnEndpointContext : EndpointContext
    {
        public ReturnEndpointContext(
            IDictionary<string, object> environment,
            ClaimsIdentity identity, 
            IDictionary<string, string> extra) : base(environment)
        {
            Identity = identity;
            Extra = extra;
        }

        public ClaimsIdentity Identity { get; set; }
        public IDictionary<string, string> Extra { get; set; }

        public string SignInAsAuthenticationType { get; set; }
        public string RedirectUri { get; set; }
    }
}