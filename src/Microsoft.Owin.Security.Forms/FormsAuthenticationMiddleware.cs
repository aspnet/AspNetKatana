using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsAuthenticationMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly FormsAuthenticationOptions _options;

        public FormsAuthenticationMiddleware(
            Func<IDictionary<string, object>, Task> next,
            FormsAuthenticationOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new FormsAuthenticationContext(_options, env);
            await context.Initialize();
            await _next(env);
            context.Teardown();
        }
    }
}
