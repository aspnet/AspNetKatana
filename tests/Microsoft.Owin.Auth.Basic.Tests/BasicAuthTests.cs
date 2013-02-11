// <copyright file="BasicAuthTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Auth.Basic.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string /*user*/, string /*psw*/, Task<bool>>;

    public class BasicAuthTests
    {
        private static readonly AppFunc NotImplemented = env => { throw new NotImplementedException(); };

        private static readonly AppFunc Success = env =>
        {
            env["owin.ResponseStatusCode"] = 200;
            return CompletedTask(true);
        };

        private static readonly AuthCallback Authenticated = (a, b, c) => CompletedTask(true);

        [Fact]
        public void Ctor_NullParameters_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BasicAuthOptions(null));
            Assert.Throws<ArgumentNullException>(() => new BasicAuthMiddleware(NotImplemented, null));
            Assert.Throws<ArgumentNullException>(() => new BasicAuthMiddleware(null, new BasicAuthOptions(Authenticated)));
        }

        [Fact]
        public void NoAuthHeader_PassesThrough()
        {
            IDictionary<string, object> environment = CreateEmptyRequest();
            var auth = new BasicAuthMiddleware(Success,
                new BasicAuthOptions((a, b, c) => { throw new NotImplementedException(); }));
            auth.Invoke(environment);

            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void BasicAuthHeader_ParsedAndDelegateInvoked()
        {
            string user = "user";
            string psw = "pwsd";
            string header = "basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + psw));
            IDictionary<string, object> environment = CreateEmptyRequest(header);
            bool callbackInvoked = false;

            var options = new BasicAuthOptions((env, usr, pass) =>
            {
                callbackInvoked = true;
                Assert.Equal(environment, env);
                Assert.Equal(user, usr);
                Assert.Equal(psw, pass);
                return CompletedTask(true);
            });
            var auth = new BasicAuthMiddleware(Success, options);
            auth.Invoke(environment);

            Assert.True(callbackInvoked);
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void SslRequired_HttpRejectedBeforePasswordValidation()
        {
            string user = "user";
            string psw = "pwsd";
            string header = "basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + psw));
            IDictionary<string, object> environment = CreateEmptyRequest(header);

            var options = new BasicAuthOptions((env, usr, pass) => { throw new NotImplementedException(); })
            { RequireEncryption = true };

            var auth = new BasicAuthMiddleware(Success, options);
            auth.Invoke(environment);

            Assert.Equal(401, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void SslRequired_HttpsAccepted()
        {
            string user = "user";
            string psw = "pwsd";
            string header = "basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + psw));
            bool callbackInvoked = false;
            IDictionary<string, object> environment = CreateEmptyRequest(header);
            environment["owin.RequestScheme"] = Uri.UriSchemeHttps;

            var options = new BasicAuthOptions((env, usr, pass) =>
            {
                callbackInvoked = true;
                Assert.Equal(environment, env);
                Assert.Equal(user, usr);
                Assert.Equal(psw, pass);
                return CompletedTask(true);
            }) { RequireEncryption = true };

            var auth = new BasicAuthMiddleware(Success, options);
            auth.Invoke(environment);

            Assert.True(callbackInvoked);
            Assert.Equal(200, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void SslRequired_HttpsWithBadPasswordRejected()
        {
            string user = "user";
            string psw = "pwsd";
            string header = "basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + psw));
            bool callbackInvoked = false;
            IDictionary<string, object> environment = CreateEmptyRequest(header);
            environment["owin.RequestScheme"] = Uri.UriSchemeHttps;

            var options = new BasicAuthOptions((env, usr, pass) =>
            {
                callbackInvoked = true;
                Assert.Equal(environment, env);
                Assert.Equal(user, usr);
                Assert.Equal(psw, pass);
                return CompletedTask(false);
            }) { RequireEncryption = true };

            var auth = new BasicAuthMiddleware(Success, options);
            auth.Invoke(environment);

            Assert.True(callbackInvoked);
            Assert.Equal(401, environment["owin.ResponseStatusCode"]);
        }

        private static Task<bool> CompletedTask(bool result)
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.TrySetResult(result);
            return tcs.Task;
        }

        private IDictionary<string, object> CreateEmptyRequest(string authHeader = null)
        {
            IDictionary<string, object> environment = new Dictionary<string, object>();
            var requestHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            environment["owin.RequestHeaders"] = requestHeaders;
            if (authHeader != null)
            {
                requestHeaders["Authorization"] = new string[] { authHeader };
            }
            environment["owin.ResponseHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            environment["server.OnSendingHeaders"] = new Action<Action<object>, object>((a, b) => { });
            return environment;
        }
    }
}
