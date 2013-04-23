using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace Microsoft.Owin
{
    public interface IAuthenticationHandler
    {
        string AuthenticationType { get; }
        IDictionary<string, object> Description { get; }
        Task<AuthenticationData> Authenticate();
    }
}
