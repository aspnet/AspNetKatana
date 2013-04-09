using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Facebook
{
    public class FacebookAuthenticationProvider : IFacebookAuthenticationProvider
    {
        public FacebookAuthenticationProvider()
        {
            OnValidateLogin = async _ => { };
        }

        public Func<FacebookValidateLoginContext, Task> OnValidateLogin { get; set; }

        public virtual Task ValidateLogin(FacebookValidateLoginContext context)
        {
            return OnValidateLogin(context);
        }
    }
}