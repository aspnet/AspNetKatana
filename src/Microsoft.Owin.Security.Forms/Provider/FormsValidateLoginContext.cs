using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsValidateLoginContext
    {
        public FormsValidateLoginContext(IDictionary<string, object> environment, string authenticationType, string name, string password)
        {
            Environment = environment;
            AuthenticationType = authenticationType;
        }

        public IDictionary<string, object> Environment { get; set; }
        public string AuthenticationType { get; private set; }

        public IIdentity Identity { get; private set; }

        public async Task<IDictionary<string, string>> ReadFormDataAsync()
        {
            return null;
        }

        public void Signin(IIdentity identity)
        {
            Identity = identity;
        }

        public void Signin(string name, params Claim[] claims)
        {
            Signin(name, (IEnumerable<Claim>)claims);
        }

        public void Signin(string name, IEnumerable<Claim> claims)
        {
            Identity = new ClaimsIdentity(new GenericIdentity(name, AuthenticationType), claims);
        }
    }
}