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
using System.Threading.Tasks;
using System.Web;

namespace Katana.Boot.AspNet
{
    public class AspNetCaller
    {
        private readonly Func<IDictionary<string, object>, Task> _next;

        public AspNetCaller(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var workerRequest = new KatanaWorkerRequest(env);
            HttpRuntime.ProcessRequest(workerRequest);
            return workerRequest.Completed;
        }
    }
}
