using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Facebook
{
    public class FacebookReturnEndpointContext : ReturnEndpointContext
    {
        public FacebookReturnEndpointContext(IDictionary<string, object> environment, ClaimsIdentity identity, IDictionary<string, string> extra) : base(environment, identity, extra)
        {
        }
    }
}