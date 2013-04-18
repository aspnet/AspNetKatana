using System.Collections.Generic;
using System.Security.Claims;
using System.Xml.Linq;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Google
{
    public class GoogleAuthenticatedContext : BaseContext
    {
        public GoogleAuthenticatedContext(
            IDictionary<string, object> environment, 
            ClaimsIdentity identity,
            IDictionary<string, string> extra,
            XElement responseMessage,
            IDictionary<string, string> attributeExchangeProperties)
            : base(environment)
        {
            Identity = identity;
            Extra = extra;
            ResponseMessage = responseMessage;
            AttributeExchangeProperties = attributeExchangeProperties;
        }

        public ClaimsIdentity Identity { get; set; }
        public IDictionary<string, string> Extra { get; set; }

        public XElement ResponseMessage { get; set; }
        public IDictionary<string, string> AttributeExchangeProperties { get; private set; }
    }
}