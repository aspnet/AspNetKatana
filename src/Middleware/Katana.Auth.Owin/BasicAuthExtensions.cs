//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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