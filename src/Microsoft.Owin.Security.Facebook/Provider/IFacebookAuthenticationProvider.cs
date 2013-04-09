using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Facebook
{
    public interface IFacebookAuthenticationProvider
    {
        Task ValidateLogin(FacebookValidateLoginContext context);
    }
}
