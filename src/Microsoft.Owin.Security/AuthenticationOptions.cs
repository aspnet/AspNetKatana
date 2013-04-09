namespace Microsoft.Owin.Security
{
    public abstract class AuthenticationOptions
    {
        protected AuthenticationOptions(string authenticationType)
        {
            AuthenticationType = authenticationType;
            AuthenticationMode = AuthenticationMode.Active;
        }

        public string AuthenticationType { get; set; }
        public AuthenticationMode AuthenticationMode { get; set; }
    }
}