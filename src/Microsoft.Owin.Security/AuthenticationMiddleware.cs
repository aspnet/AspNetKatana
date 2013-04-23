using System.Threading.Tasks;

namespace Microsoft.Owin.Security
{
    public abstract class AuthenticationMiddleware<TOptions> : OwinMiddleware where TOptions : AuthenticationOptions
    {
        public TOptions Options { get; set; }

        public AuthenticationMiddleware(OwinMiddleware next, TOptions options)
            : base(next)
        {
            Options = options;
        }

        public override async Task Invoke(OwinRequest request, OwinResponse response)
        {
            var handler = CreateHandler();
            await handler.Initialize(Options, request, response);
            if (!await handler.Invoke())
            {
                await base.Invoke(request, response);
            }
            await handler.Teardown();
        }

        protected abstract AuthenticationHandler<TOptions> CreateHandler();
    }
}