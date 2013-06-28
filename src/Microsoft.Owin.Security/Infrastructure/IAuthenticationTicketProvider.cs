using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Infrastructure
{
    public interface IAuthenticationTicketProvider
    {
        void Creating(AuthenticationTicketProviderContext context);
        Task CreatingAsync(AuthenticationTicketProviderContext context);
        void Consuming(AuthenticationTicketProviderContext context);
        Task ConsumingAsync(AuthenticationTicketProviderContext context);
    }
}
