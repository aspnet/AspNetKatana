using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.WebApi.Owin;
using Owin;

namespace Microsoft.AspNet.Owin.WebApplication
{
    public static class AuthorizeRoleFilterExtensions
    {
        public static IAppBuilder UseTraceRequestFilter(this IAppBuilder builder)
        {
            return builder.UseMessageHandler(inner => new TraceRequestFilter(inner));
        }
        public static IAppBuilder UseAuthorizeRoleFilter(this IAppBuilder builder, string role)
        {
            return builder.UseMessageHandler(innerHandler => new AuthorizeRoleFilter(innerHandler, role));
        }
    }

    public class AuthorizeRoleFilter : DelegatingHandler
    {
        public string Role { get; set; }

        public AuthorizeRoleFilter(string role)
        {
            Role = role;
        }

        public AuthorizeRoleFilter(HttpMessageHandler innerHandler, string role)
            : base(innerHandler)
        {
            Role = role;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (!Thread.CurrentPrincipal.IsInRole(Role))
            {
                return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}