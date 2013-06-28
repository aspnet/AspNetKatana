using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class AuthenticationTicketProviderContextTests
    {
        [Fact]
        public void ContextCanBeCreated()
        {
            var context = CreateContext();
            context.ShouldNotBe(null);
        }

        [Fact]
        public void ReferenceIdCanBeChanged()
        {
            var context = CreateContext();
            context.TokenValue.ShouldBe(null);
            context.TokenValue = "Anything";
            context.TokenValue.ShouldBe("Anything");
        }

        [Fact]
        public void TicketDataIsAvailableAfterIdentityIsProvided()
        {
            var context = CreateContext();
            context.ProtectedData.ShouldBe(null);
            context.Ticket.ShouldBe(null);
            context.Identity.ShouldBe(null);
            context.Extra.ShouldBe(null);
            context.SetTicket(new ClaimsIdentity("Bearer"), new AuthenticationExtra { RedirectUrl = "hello" });            
            context.ProtectedData.ShouldNotBe(null);
            context.Ticket.ShouldNotBe(null);
            context.Identity.ShouldNotBe(null);
            context.Extra.ShouldNotBe(null);
            context.Extra.RedirectUrl.ShouldBe("hello");
        }


        [Fact]
        public void TicketValuesAreAvailableAfterDataIsProvided()
        {
            var initial = CreateContext();
            initial.SetTicket(new ClaimsIdentity("Bearer"), new AuthenticationExtra { RedirectUrl = "hello" });

            var context = CreateContext();
            context.ProtectedData.ShouldBe(null);
            context.Ticket.ShouldBe(null);
            context.Identity.ShouldBe(null);
            context.Extra.ShouldBe(null);
            context.SetProtectedData(initial.ProtectedData);
            context.ProtectedData.ShouldNotBe(null);
            context.Ticket.ShouldNotBe(null);
            context.Identity.ShouldNotBe(null);
            context.Extra.ShouldNotBe(null);
            context.Extra.RedirectUrl.ShouldBe("hello");
        }

        private static AuthenticationTicketProviderContext CreateContext()
        {
            return new AuthenticationTicketProviderContext(new TicketDataHandler(new TestDataProtector()));
        }
    }

    public class TestDataProtector : IDataProtector
    {
        public byte[] Protect(byte[] userData)
        {
            return userData.Select(b => ((byte)(b + 13))).ToArray();
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return protectedData.Select(b => ((byte)(b - 13))).ToArray();
        }
    }
}
