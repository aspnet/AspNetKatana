// <copyright file="BasicAuthTests.cs" company="Katana contributors">
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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Auth.Basic.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class BasicAuthTests
    {
        private static readonly AppFunc NotImplemented = env => { throw new NotImplementedException(); };
        private static readonly AppFunc Success = env => { env["owin.ResponseStatusCode"] = 200; return null; };

        [Fact]
        public void Ctor_NullParameters_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicAuthMiddleware(null, new BasicAuthMiddleware.Options()));
            Assert.Throws<ArgumentNullException>(() => new BasicAuthMiddleware(NotImplemented, null));
        }

        [Fact]
        public void NoAuthHeader_PassesThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            BasicAuthMiddleware auth = new BasicAuthMiddleware(Success, new BasicAuthMiddleware.Options());
            auth.Invoke(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void BasicAuthHeader_ParsedAndDelegateInvoked()
        {
            string user = "user";
            string psw = "pwsd";
            string header = "basic: " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + psw));
            IDictionary<string, object> environment = CreateEmptyRequest(header);

            BasicAuthMiddleware.Options options = new BasicAuthMiddleware.Options();
            options.Authenticate = (env, usr, pass) =>
            {
                Assert.Equal(environment, env);
                Assert.Equal(user, usr);
                Assert.Equal(psw, pass);
                return CompletedTask(true);
            };
            BasicAuthMiddleware auth = new BasicAuthMiddleware(Success, options);
            auth.Invoke(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        private Task<bool> CompletedTask(bool result)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            tcs.TrySetResult(result);
            return tcs.Task;
        }

        private IDictionary<string, object> CreateEmptyRequest(string authHeader = null)
        {
            IDictionary<string, object> environment = new Dictionary<string, object>();
            Dictionary<string, string[]> requestHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment["owin.RequestHeaders"] = requestHeaders;
            if (authHeader != null)
            {
                requestHeaders["Authorization"] = new string[] { authHeader };
            }

            environment["server.OnSendingHeaders"] = new Action<Action<object>, object>((a, b) => { });
            return environment;
        }
    }
}
