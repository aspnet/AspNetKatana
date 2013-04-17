using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json.Linq;

namespace Microsoft.Owin.Security.Facebook
{
    public class FacebookAuthenticatedContext : BaseContext
    {
        public FacebookAuthenticatedContext(IDictionary<string, object> environment, JObject user, string accessToken) : base(environment)
        {
            User = user;
            AccessToken = accessToken;

            Id = User["id"].ToString();
            Name = User["name"].ToString();
            Link = User["link"].ToString();
            Username = User["username"].ToString();
            Email = User["email"].ToString();
        }

        public JObject User { get; private set; }
        public string AccessToken { get; private set; }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Link { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        public ClaimsIdentity Identity { get; set; }
        public IDictionary<string, string> Extra { get; set; }
    }
}