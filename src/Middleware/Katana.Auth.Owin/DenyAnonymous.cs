using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace Katana.Auth.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware can be placed at the end of a chain of pass-through auth schemes if at least one type of auth is required.
    public class DenyAnonymous
    {
        private AppFunc nextApp;

        public DenyAnonymous(AppFunc nextApp)
        {
            this.nextApp = nextApp;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            if (env.Get<IPrincipal>(Constants.ServerUserKey) == null)
            {
                env[Constants.ResponseStatusCodeKey] = 401;
                return TaskHelpers.Completed();
            }

            return nextApp(env);
        }
    }
}
