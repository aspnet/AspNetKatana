using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    public interface IFormsAuthenticationProvider
    {
        Task ValidateLogin(FormsValidateLoginContext context);
        Task ValidateIdentity(FormsValidateIdentityContext context);
    }
}
