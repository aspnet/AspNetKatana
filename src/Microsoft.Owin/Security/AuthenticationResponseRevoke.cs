namespace Microsoft.Owin.Security
{
    public class AuthenticationResponseRevoke
    {
        public AuthenticationResponseRevoke(string[] authenticationTypes)
        {
            AuthenticationTypes = authenticationTypes;
        }

        public string[] AuthenticationTypes { get; private set; }
    }
}