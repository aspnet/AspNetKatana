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
using Katana.Auth.Owin;

namespace Owin
{
    using AuthCallback = Func<IDictionary<string, object> /*env*/, string/*user*/, string/*psw*/, Task<bool>>;

    public static class BasicAuthExtensions
    {
        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, BasicAuth.Options options)
        {
            return builder.UseType<BasicAuth>(options);
        }

        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, AuthCallback authenticate)
        {
            var options = new BasicAuth.Options
            {
                Authenticate = authenticate
            };
            return builder.UseType<BasicAuth>(options);
        }

        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, Func<string, string, Task<bool>> authenticate, string realm)
        {
            var options = new BasicAuth.Options
            {
                Realm = realm,
                Authenticate = (env, user, pass) => authenticate(user, pass)
            };
            return builder.UseType<BasicAuth>(options);
        }

        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, AuthCallback authenticate, string realm)
        {
            var options = new BasicAuth.Options
            {
                Realm = realm,
                Authenticate = authenticate
            };
            return builder.UseType<BasicAuth>(options);
        }

        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, Func<string, string, Task<bool>> authenticate, bool requireEncryption)
        {
            var options = new BasicAuth.Options
            {
                RequireEncryption = requireEncryption,
                Authenticate = (env, user, pass) => authenticate(user, pass)
            };
            return builder.UseType<BasicAuth>(options);
        }

        public static IAppBuilder UseBasicAuth(this IAppBuilder builder, AuthCallback authenticate, bool requireEncryption)
        {
            var options = new BasicAuth.Options
            {
                RequireEncryption = requireEncryption,
                Authenticate = authenticate
            };
            return builder.UseType<BasicAuth>(options);
        }
    }
}