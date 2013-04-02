// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IdentityModel.Tokens;
using Microsoft.AspNet.Security;
using Microsoft.Owin.Security;

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
            UseBearerAuthentication(appBuilder, new IdentityModelBearerAuthenticationProvider(handlers));
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="handlers"></param>
        /// <param name="realm"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder,
            SecurityTokenHandlerCollection handlers, string realm)
        {
            UseBearerAuthentication(appBuilder, new IdentityModelBearerAuthenticationProvider(handlers), realm);
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="provider"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder, IBearerAuthenticationProvider provider)
        {
            UseBearerAuthentication(appBuilder, provider, null);
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

            UseBearerAuthentication(appBuilder, new BearerAuthenticationProtocol(provider, realm));
        }

        /// <summary></summary>
        /// <param name="appBuilder"></param>
        /// <param name="protocol"></param>
        public static void UseBearerAuthentication(this IAppBuilder appBuilder, IBearerAuthenticationProtocol protocol)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            BearerAuthenticationOptions options = new BearerAuthenticationOptions
            {
                Protocol = protocol
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
