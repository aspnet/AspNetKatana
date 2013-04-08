// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security.Bearer;

namespace Owin
{
    /// <summary></summary>
    public static class BearerAuthenticationExtensions
    {
        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="handlers"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder,
            SecurityTokenHandlerCollection handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException("handlers");
            }

            BearerAuthenticationOptions options = new BearerAuthenticationOptions
            {
                Provider = new IdentityModelBearerAuthenticationProvider(handlers)
            };

            UseBearerAuthentication(appBuilder, options);
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="handlers"></param>
        /// <param name="realm"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder,
            SecurityTokenHandlerCollection handlers, string realm)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException("handlers");
            }

            BearerAuthenticationOptions options = new BearerAuthenticationOptions
            {
                Provider = new IdentityModelBearerAuthenticationProvider(handlers),
                Realm = realm
            };

            UseBearerAuthentication(appBuilder, options);
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="provider"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder, IBearerAuthenticationProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            BearerAuthenticationOptions options = new BearerAuthenticationOptions
            {
                Provider = provider
            };

            UseBearerAuthentication(appBuilder, options);
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="provider"></param>
        /// <param name="realm"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder, IBearerAuthenticationProvider provider,
            string realm)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            BearerAuthenticationOptions options = new BearerAuthenticationOptions
            {
                Provider = provider,
                Realm = realm
            };

            UseBearerAuthentication(appBuilder, options);
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="options"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder, BearerAuthenticationOptions options)
        {
            if (appBuilder == null)
            {
                throw new ArgumentNullException("appBuilder");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            appBuilder.Use(typeof(BearerAuthenticationMiddleware), options);
            appBuilder.MarkStage("Authenticate");
        }
    }
}
