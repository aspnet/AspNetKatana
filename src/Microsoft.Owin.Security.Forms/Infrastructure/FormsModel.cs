using System;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Forms.Infrastructure
{
    public class FormsModel
    {
        public ClaimsPrincipal Principal { get; set; }
        public bool IsPersistent { get; set; }
        public DateTimeOffset ExpireUtc { get; set; }
    }
}