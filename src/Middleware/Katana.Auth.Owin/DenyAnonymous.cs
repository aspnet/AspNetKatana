// Copyright 2011-2012 Katana contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Katana.Auth.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // This middleware can be placed at the end of a chain of pass-through auth schemes if at least one type of auth is required.
    public class DenyAnonymous
    {
        private readonly AppFunc nextApp;

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
