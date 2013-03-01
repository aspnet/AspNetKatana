using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Throttling
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ThrottlingMiddleware
    {
        private readonly AppFunc _next;
        private readonly ThrottlingOptions _options;

        public ThrottlingMiddleware(AppFunc next, ThrottlingOptions options)
        {
            _next = next;
            _options = options;
        }

        public Task Invoke(IDictionary<string,object> env)
        {
            return _next.Invoke(env);
        }
    }
}
