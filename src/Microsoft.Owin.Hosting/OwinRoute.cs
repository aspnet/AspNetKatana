// <copyright file="OwinRoute.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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
using System.Threading.Tasks;

namespace Microsoft.Owin.Hosting
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class OwinRoute
    {
        private AppFunc _next;
        private AppFunc _branch;
        private string _pathMatch;

        public OwinRoute(AppFunc next, AppFunc branch, string pathMatch)
        {
            _next = next;
            _branch = branch;
            _pathMatch = pathMatch;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            throw new NotImplementedException();
        }
    }
}
