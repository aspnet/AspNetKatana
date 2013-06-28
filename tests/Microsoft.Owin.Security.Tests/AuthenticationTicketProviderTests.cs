using Microsoft.Owin.Security.Infrastructure;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class AuthenticationTicketProviderTests
    {
        [Fact]
        public void CallingAnyMethodDoesNothingByDefault()
        {
            var provider = new AuthenticationTicketProvider();
            provider.Creating(new AuthenticationTicketProviderContext(null));
        }
    }
}
