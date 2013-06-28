using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Infrastructure
{
    public class AuthenticationTicketProvider : IAuthenticationTicketProvider
    {
        public Action<AuthenticationTicketProviderContext> OnCreating { get; set; }
        public Func<AuthenticationTicketProviderContext, Task> OnCreatingAsync { get; set; }
        public Action<AuthenticationTicketProviderContext> OnConsuming { get; set; }
        public Func<AuthenticationTicketProviderContext, Task> OnConsumingAsync { get; set; }

        public virtual void Creating(AuthenticationTicketProviderContext context)
        {
            if (OnCreatingAsync != null && OnCreating == null)
            {
                throw new InvalidOperationException(Resources.Exception_AuthenticationTicketDoesNotProvideSyncMethods);
            }
            if (OnCreating != null)
            {
                OnCreating.Invoke(context);
            }
        }

        public virtual async Task CreatingAsync(AuthenticationTicketProviderContext context)
        {
            if (OnCreatingAsync != null && OnCreating == null)
            {
                throw new InvalidOperationException(Resources.Exception_AuthenticationTicketDoesNotProvideSyncMethods);
            }
            if (OnCreatingAsync != null)
            {
                await OnCreatingAsync.Invoke(context);
            }
            else
            {
                Creating(context);
            }
        }

        public virtual void Consuming(AuthenticationTicketProviderContext context)
        {
            if (OnConsuming != null && OnConsuming == null)
            {
                throw new InvalidOperationException(Resources.Exception_AuthenticationTicketDoesNotProvideSyncMethods);
            }

            if (OnConsuming != null)
            {
                OnConsuming.Invoke(context);
            }
        }

        public virtual async Task ConsumingAsync(AuthenticationTicketProviderContext context)
        {
            if (OnConsuming != null && OnConsuming == null)
            {
                throw new InvalidOperationException(Resources.Exception_AuthenticationTicketDoesNotProvideSyncMethods);
            }
            if (OnConsumingAsync != null)
            {
                await OnConsumingAsync.Invoke(context);
            }
            else
            {
                Consuming(context);
            }
        }
    }
}