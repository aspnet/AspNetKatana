using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Newtonsoft.Json.Linq;

namespace Microsoft.Owin.Security.Facebook
{
    public class FacebookValidateLoginContext
    {
        public FacebookValidateLoginContext(IDictionary<string, object> environment, JObject user, string accessToken)
        {
            Environment = environment;
            User = user;
            AccessToken = accessToken;

            Id = User["id"].ToString();
            Name = User["name"].ToString();
            Link = User["link"].ToString();
            Username = User["username"].ToString();
            Email = User["email"].ToString();
        }

        public IDictionary<string, object> Environment { get; private set; }
        public JObject User { get; private set; }
        public string AccessToken { get; private set; }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Link { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        public IPrincipal Principal { get; private set; }

        public void Signin(IPrincipal principal)
        {
            Principal = principal;
        }
    }
}