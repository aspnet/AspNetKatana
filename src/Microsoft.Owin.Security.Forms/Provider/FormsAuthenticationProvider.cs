using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationProvider : IFormsAuthenticationProvider
    {
        public FormsAuthenticationProvider()
        {
            OnValidateLogin = async context => { };
            OnValidateIdentity = async context => { };
        }

        public Func<FormsValidateLoginContext, Task> OnValidateLogin { get; set; }

        public Func<FormsValidateIdentityContext, Task> OnValidateIdentity { get; set; }

        public virtual Task ValidateLogin(FormsValidateLoginContext context)
        {
            return OnValidateLogin.Invoke(context);
        }

        public virtual Task ValidateIdentity(FormsValidateIdentityContext context)
        {
            return OnValidateIdentity.Invoke(context);
        }
    }
}
