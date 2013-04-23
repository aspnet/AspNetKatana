using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

// ReSharper disable CheckNamespace

namespace Microsoft.Owin
{
    using AuthenticateCallback = Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>;
    using AuthenticateDelegate = Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task>;

    public static class OwinRequestExtensions
    {
        public static object HookAuthentication(this OwinRequest request, IAuthenticationHandler handler)
        {
            var chained = request.Get<AuthenticateDelegate>("security.Authenticate");
            var hook = new Hook(handler, chained);
            request.Set<AuthenticateDelegate>("security.Authenticate", hook.Authenticate);
            Trace.WriteLine(string.Format("Hook old {0} new {1}", chained, hook));
            return hook;
        }

        public static void UnhookAuthentication(this OwinRequest request, object state)
        {
            var hook = (Hook)state;
            Trace.WriteLine(string.Format("Unhook old {0} new {1}", request.Get<AuthenticateDelegate>("security.Authenticate"), hook));
            request.Set("security.Authenticate", hook.Chained);
        }

        public class Hook
        {
            private readonly IAuthenticationHandler _handler;
            public AuthenticateDelegate Chained { get; private set; }

            public Hook(IAuthenticationHandler handler, AuthenticateDelegate chained)
            {
                _handler = handler;
                Chained = chained;
            }

            public async Task Authenticate(
                string[] authenticationTypes,
                AuthenticateCallback callback,
                object state)
            {
                if (authenticationTypes == null)
                {
                    callback(null, null, _handler.Description, state);
                }
                else if (authenticationTypes.Contains(_handler.AuthenticationType, StringComparer.Ordinal))
                {
                    var model = await _handler.Authenticate();
                    if (model != null)
                    {
                        callback(model.Identity, model.Extra, _handler.Description, state);
                    }
                }
                if (Chained != null)
                {
                    await Chained(authenticationTypes, callback, state);
                }
            }
        }
    }
}
